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

        private Timer Timer;

        public VehicleProcessor(SyncromaticsService service, Dictionary<int, Route> routes)
        {
            Syncromatics = service;
            Database = service.GetDatabase();
            Routes = routes;
            
            VehicleStates = new Dictionary<int, VehicleState>();
            InProgressHistories = new Dictionary<int, TripHistory>();
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
                    else await HandleNewVehicle(vehicle);

                    /*
                    if (InProgressHistories.ContainsKey(vehicle.ID))
                    {
                        //vehicle already visited a stop -- let us make sure it is not the same stop.
                        //if it is, we should mark it as dwell time.
                        TripHistory history = InProgressHistories[vehicle.ID];
                        Stop stop = Spatial.StopResolver.GetVehicleExpectedStop(Routes[key], vehicle, state);
                        if (stop != null)
                        {
                            if (stop.StopID != history.OriginStopID)
                            {
                                if (vehicle.ID == 482) Console.WriteLine("vehicle 1331 arrived at destination stop " + stop.StopName);

                                history.DestinationStopID = stop.StopID;
                                history.TimeArrivedDestination = DateTime.Parse(vehicle.AcceptableUpdated());
                                history.TimeBucket = Database.GetTripHistoryCollection().GetCurrentTimeBucket();
                                await Database.GetTripHistoryCollection().InsertTripHistory(history);
                                InProgressHistories.Remove(vehicle.ID);

                                TripHistory newHistory = new TripHistory();
                                newHistory.RouteID = Routes[key].RouteID;
                                newHistory.OriginStopID = stop.StopID;
                                newHistory.TimeLeftOrigin = DateTime.UnixEpoch;
                                InProgressHistories.Add(vehicle.ID, newHistory);
                            }
                        }
                        else if (stop == null && history.TimeLeftOrigin == DateTime.UnixEpoch)
                        {
                            //vehicle is not at that stop anymore but time left origin wasn't recorded yet.
                            history.TimeLeftOrigin = DateTime.Parse(vehicle.AcceptableUpdated());
                            if (vehicle.ID == 482) Console.WriteLine("vehicle 1331 left stop, updating time");
                        }
                    }
                    else
                    {
                        Stop stop = StopResolver.GetCurrentStop(Routes[key], vehicle);
                        if (stop != null)
                        {
                            if (vehicle.ID == 482) Console.WriteLine("vehicle 1331 arrived at first stop " + stop.StopName);

                            //freshly arrived at a stop
                            TripHistory newHistory = new TripHistory();
                            newHistory.RouteID = Routes[key].RouteID;
                            newHistory.OriginStopID = stop.StopID;
                            newHistory.TimeLeftOrigin = DateTime.UnixEpoch;
                            InProgressHistories.Add(vehicle.ID, newHistory);
                        }
                        //ignore if no stops found, hasn't gotten anywhere yet
                    }
                    */
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        private async Task HandleNewVehicle(SyncromaticsVehicle vehicle)
        {
            VehicleState state = new VehicleState(vehicle);
            Route route = Routes[vehicle.RouteID];
            Stop stop = StopResolver.GetVehicleStop(route, state, true);
            if (stop == null) return; //we aren't interested in vehicles that haven't yet reached a stop.

            if (vehicle.Name.Equals("4009")) Console.WriteLine($"Vehicle {vehicle.Name} has reached their first stop: {stop.StopName}");

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
                    //vehicle is at a different stop
                    if (vehicle.Name.Equals("4009")) Console.WriteLine($"Vehicle {vehicle.Name} has reached their stop: {stop.StopName}");

                    TripHistory history = InProgressHistories[vehicle.ID];
                    history.DestinationStopID = stop.StopID;
                    history.TimeArrivedDestination = DateTime.Parse(vehicle.AcceptableUpdated());
                    history.TimeBucket = Database.GetTripHistoryCollection().GetCurrentTimeBucket();

                    /*
                     * TODO: handle cases where refresh didn't catch leaving the last stop.
                     * probably need to extrapolate the vehicle's speed (it's unlikely that it stopped)
                     * and use the time it arrived at this stop to figure out when it left the last stop?
                     * 
                     * this usually happens when two stops are in close proximity, and the 3s polling rate
                     * isn't enough to detect when one is left before it arrives at the second one
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

                    state.IncrementStopIndex(route);
                    if (history.TimeLeftOrigin != DateTime.UnixEpoch) await Database.GetTripHistoryCollection().InsertTripHistory(history);
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
                    if (vehicle.Name.Equals("4009")) Console.WriteLine($"Vehicle {vehicle.Name} has left their stop");
                    InProgressHistories[vehicle.ID].TimeLeftOrigin = DateTime.Parse(vehicle.AcceptableUpdated());
                }
            }
        }


    }
}
