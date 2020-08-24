using BetterBullTracker.Models;
using BetterBullTracker.Models.HistoricalRecords;
using BetterBullTracker.Models.Syncromatics;
using BetterBullTracker.Services;
using BetterBullTracker.Spatial;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace BetterBullTracker.Syncromatics
{
    public class VehicleProcessor
    {
        SyncromaticsService Syncromatics;
        DatabaseService Database;

        private Dictionary<int, VehicleState> VehicleStates;
        private Dictionary<int, TripHistory> InProgressHistories;
        private Dictionary<int, Route> Routes;
        private List<int> MissingVehicles;

        private Timer Timer;

        public VehicleProcessor(SyncromaticsService service, Dictionary<int, Route> routes)
        {
            Syncromatics = service;
            Database = service.GetDatabase();
            Routes = routes;
            
            VehicleStates = new Dictionary<int, VehicleState>();
            InProgressHistories = new Dictionary<int, TripHistory>();
            MissingVehicles = new List<int>();
        }

        public void Start()
        {
            Timer = new Timer(3000);
            Timer.AutoReset = true;
            Timer.Elapsed += new ElapsedEventHandler(DownloadLatestVehicles);
            Timer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private async void DownloadLatestVehicles(Object source, ElapsedEventArgs e)
        {
            string URL = Syncromatics.GetURL();

            foreach(Route route in Routes.Values)
            {
                List<SyncromaticsVehicle> vehicles = await $"{URL}/Route/{route.RouteID}/Vehicles".GetJsonAsync<List<SyncromaticsVehicle>>();
                foreach (SyncromaticsVehicle vehicle in vehicles)
                {
                    if (VehicleStates.ContainsKey(vehicle.ID)) await HandleExistingVehicle(vehicle);
                    else HandleNewVehicle(vehicle);
                }

                /*
                 * At closing time, buses will straight up drop out of Syncromatic's view nearly immediately.
                 * Because this runs all the time, we want to make sure we don't mix up routes when buses restart
                 * the next day, and we also want to ensure that buses that take a break (and switch routes or turn off)
                 * aren't being tracked.
                 */
                foreach (VehicleState state in VehicleStates.Values.ToList().FindAll(x => x.RouteID == route.RouteID))
                {
                    if (vehicles.FindIndex(x => x.ID == state.ID) != -1)
                    {
                        if (MissingVehicles.Contains(state.ID)) MissingVehicles.Remove(state.ID);
                    }
                    else
                    {
                        if (MissingVehicles.Contains(state.ID))
                        {
                            Console.WriteLine($"Vehicle {state.BusNumber} was not found twice, removing.");

                            MissingVehicles.Remove(state.ID);
                            if (InProgressHistories.ContainsKey(state.ID)) InProgressHistories.Remove(state.ID);
                            VehicleStates.Remove(state.ID);
                        }
                        else MissingVehicles.Add(state.ID);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        private void HandleNewVehicle(SyncromaticsVehicle vehicle)
        {
            VehicleState state = new VehicleState(vehicle);
            Route route = Routes[vehicle.RouteID];
            Stop stop = StopResolver.GetVehicleStop(route, state);
            
            /*
             * if this vehicle is new, we want to make sure there's no wonkiness
             * by only starting to track it if it has reached the 1st stop on its route,
             * either the msc or the library.
             */
            if (stop == null || stop.StopID != route.RouteStops[0].StopID) return;

            TripHistory history = new TripHistory();
            history.RouteID = vehicle.RouteID;
            history.OriginStopID = stop.StopID;
            history.TimeLeftOrigin = DateTime.UnixEpoch;

            InProgressHistories.Add(vehicle.ID, history);
            VehicleStates.Add(vehicle.ID, state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        private async Task HandleExistingVehicle(SyncromaticsVehicle vehicle)
        {
            VehicleState state = VehicleStates[vehicle.ID];
            if (state.GetLatestVehicleReport().Updated.Equals(vehicle.Updated)) return; //we aren't interested in reports that haven't been updated
            state.AddVehicleReport(vehicle);

            Route route = Routes[vehicle.RouteID];
            Stop stop = StopResolver.GetVehicleStop(route, state);
            
            if (stop != null)
            {
                //vehicle has reached a stop
                if (InProgressHistories.ContainsKey(vehicle.ID) && InProgressHistories[vehicle.ID].OriginStopID != stop.StopID)
                {
                    TripHistory history = InProgressHistories[vehicle.ID];

                    //vehicle is at a different stop
                    //lets make sure it didn't skip a stop
                    int originalStopIndex = route.GetIndexByStopID(history.OriginStopID);
                    int thisStopIndex = route.GetIndexByStopID(stop.StopID);

                    history.DestinationStopID = stop.StopID;
                    history.TimeArrivedDestination = DateTime.Parse(vehicle.AcceptableUpdated());
                    history.TimeBucket = Database.GetTripHistoryCollection().GetCurrentTimeBucket();

                    /*
                     * TODO: handle cases where refresh didn't catch leaving the last stop.
                     * probably need to extrapolate the vehicle's speed (it's unlikely that it stopped)
                     * and use the time it arrived at this stop to figure out when it left the last stop?
                     * 
                     * this usually happens when two stops are in close proximity, and the 3s polling rate
                     * isn't enough to detect when the bus left the first before it arrives at the second one
                     * 
                     * or we can throw away the trip history, since we'll probably have spares. gonna do
                     * that for now, because vehicles obviously can't teleport between stops
                     */

                    TripHistory newHistory = new TripHistory();
                    newHistory.RouteID = vehicle.RouteID;
                    newHistory.OriginStopID = stop.StopID;
                    newHistory.TimeLeftOrigin = DateTime.UnixEpoch;

                    InProgressHistories.Remove(vehicle.ID);
                    InProgressHistories.Add(vehicle.ID, newHistory);
;
                    /*
                     * due to the 3s interval before we call vehicle updates, we can sometimes miss when a vehicle arrived/departed at a stop
                     * if it is going fast enough. We still want to increase the stop index so we have an accurate representation of what
                     * stops vehicles have passed or not.
                     * 
                     * the reason for the weird math after the || is because we don't want to trigger this when the bus gets back to its original stop
                     */
                    if (thisStopIndex - originalStopIndex != 1 || Math.Abs(thisStopIndex - originalStopIndex) != route.RouteStops.Count - 1)
                    {
                        Console.WriteLine($"Vehicle {vehicle.Name} has missed a stop!");
                        for (int i = 0; i < thisStopIndex - originalStopIndex; i++) state.IncrementStopIndex(route);
                    }
                    else state.IncrementStopIndex(route);

                    if (history.TimeLeftOrigin != DateTime.UnixEpoch && (thisStopIndex - originalStopIndex == 1 || Math.Abs(thisStopIndex - originalStopIndex) != route.RouteStops.Count - 1)) await Database.GetTripHistoryCollection().InsertTripHistory(history);
                    else Console.WriteLine("a trip history was thrown out!"); //see above
                }
                else
                {
                    //vehicle is at the same stop. for now, don't do anything, but we could probably record dwell time eventually
                }
            }
            else
            {
                //vehicle is not currently at a stop, but if it was before, let's record when it left.
                if (InProgressHistories.ContainsKey(vehicle.ID) && InProgressHistories[vehicle.ID].TimeLeftOrigin == DateTime.UnixEpoch)
                {
                    InProgressHistories[vehicle.ID].TimeLeftOrigin = DateTime.Parse(vehicle.AcceptableUpdated());
                }
            }
        }


    }
}
