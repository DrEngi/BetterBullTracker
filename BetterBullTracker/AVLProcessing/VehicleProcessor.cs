using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Databases;
using BetterBullTracker.Databases.Models;
using BetterBullTracker.Spatial;
using Flurl.Http;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace BetterBullTracker.AVLProcessing
{
    public class VehicleProcessor
    {
        AVLProcessingService AVLProcessing;
        DatabaseService Database;

        private Dictionary<int, VehicleState> VehicleStates;
        private Dictionary<int, TripHistory> InProgressHistories;
        private Dictionary<int, Route> Routes;

        public VehicleProcessor(AVLProcessingService service, Dictionary<int, Route> routes)
        {
            AVLProcessing = service;
            Database = service.GetDatabase();
            Routes = routes;

            VehicleStates = new Dictionary<int, VehicleState>();
            InProgressHistories = new Dictionary<int, TripHistory>();
        }

        public void Start()
        {
            AVLProcessing.Syncromatics.NewVehicleDownloaded += async (s, e) => await Syncromatics_NewVehicleDownloadedAsync(s, e);
            AVLProcessing.Syncromatics.Start();
        }

        private async Task Syncromatics_NewVehicleDownloadedAsync(object sender, SyncromaticsAPI.Events.VehicleDownloadedArgs e)
        {
            Console.WriteLine("Processing vehicle " + e.Vehicle.Name);
            if (VehicleStates.ContainsKey(e.Vehicle.ID)) await HandleExistingVehicle(e.Vehicle);
            else HandleNewVehicle(e.Vehicle);
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
            Stop stop = SpatialMatcher.GetVehicleStop(route, state);

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

            //we have not seen this vehicle before. let's check where it is
            StopPath stopPath = SpatialMatcher.GetStopPath(route, state);

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
            if (state.GetLatestVehicleReport().RouteID != vehicle.RouteID)
            {
                //if a vehicle changes routes, remove its state and restart
                VehicleStates.Remove(vehicle.ID);
                return;
            }
            state.AddVehicleReport(vehicle);

            Route route = Routes[vehicle.RouteID];
            Stop stop = SpatialMatcher.GetVehicleStop(route, state);
            StopPath stopPath = SpatialMatcher.GetStopPath(route, state);

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
                     * TODO: BROKEN, maybe?
                     */
                    if (thisStopIndex - originalStopIndex != 1 || (thisStopIndex == 0 && originalStopIndex == route.RouteStops.Count - 1))
                    {
                        Console.WriteLine($"Vehicle {vehicle.Name} has missed a stop!");
                        //for (int i = 0; i < thisStopIndex - originalStopIndex; i++) state.IncrementStopIndex(route);
                    }
                    else state.IncrementStopIndex(route);

                    if (history.TimeLeftOrigin != DateTime.UnixEpoch && (thisStopIndex - originalStopIndex != 1 || (thisStopIndex == 0 && originalStopIndex == route.RouteStops.Count - 1))) await Database.GetTripHistoryCollection().InsertTripHistory(history);
                    else Console.WriteLine("a trip history was thrown out!"); //see above
                }
                else
                {
                    //vehicle is at the same stop, start a new dwelltime
                }
            }
            else
            {
                //vehicle is not currently at a stop, but if it was before, let's record when it left.
                if (InProgressHistories.ContainsKey(vehicle.ID) && InProgressHistories[vehicle.ID].TimeLeftOrigin == DateTime.UnixEpoch)
                {
                    InProgressHistories[vehicle.ID].TimeLeftOrigin = DateTime.Parse(vehicle.AcceptableUpdated());
                }

                //make sure it's still on route
                if (stopPath == null)
                {
                    Console.WriteLine("Vehicle " + vehicle.Name + " not on route");
                }
            }
            await AVLProcessing.GetWebsockets().SendVehicleUpdateAsync(new WebSockets.WSVehicleUpdateMsg(state));
        }
    }
}
