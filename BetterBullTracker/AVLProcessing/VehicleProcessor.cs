using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Databases;
using BetterBullTracker.Databases.Models;
using BetterBullTracker.Spatial;
using Flurl.Http;
using MongoDB.Bson.IO;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using SyncromaticsAPI.Events;
using BetterBullTracker.AVLProcessing.VehicleHandling;
using System.Collections.Concurrent;
using System.Diagnostics;
using BetterBullTracker.Spatial.Geometry;

namespace BetterBullTracker.AVLProcessing
{
    public class VehicleProcessor
    {
        AVLProcessingService AVLProcessing;
        DatabaseService Database;

        private ConcurrentDictionary<int, VehicleState> VehicleStates;
        private ConcurrentDictionary<int, TripHistory> InProgressHistories;
        private ConcurrentDictionary<int, Route> Routes;

        ConcurrentDictionary<int, BlockingCollection<SyncromaticsVehicle>> VehicleProcessingQueues;
        List<Task> VehicleWorkers;

        public VehicleProcessor(AVLProcessingService service, Dictionary<int, Route> routes)
        {
            AVLProcessing = service;
            Database = service.GetDatabase();

            RouteProcessor processor = new RouteProcessor();
            List<SyncromaticsRoute> syncRoutes = Database.GetHistoricalCollections().GetRoutes().Result;
            Routes = new ConcurrentDictionary<int, Route>(processor.ProcessRoutes(syncRoutes).Result);

            VehicleStates = new ConcurrentDictionary<int, VehicleState>();
            InProgressHistories = new ConcurrentDictionary<int, TripHistory>();

            VehicleProcessingQueues = new ConcurrentDictionary<int, BlockingCollection<SyncromaticsVehicle>>();
            VehicleWorkers = new List<Task>();
        }

        public void Start()
        {
            /*
            AVLProcessing.Syncromatics.NewVehicleDownloaded += async (s, e) => await Syncromatics_NewVehicleDownloadedAsync(s, e);
            AVLProcessing.Syncromatics.Start();
            */

            System.Timers.Timer Timer = new System.Timers.Timer(500);
            Timer.AutoReset = true;
            Timer.Elapsed += new ElapsedEventHandler(TestTriggerAsync);
            Timer.Start();
        }

        //history downloader started at this index for some reason
        //int i = 157;
        int i = 350;
        int failedMatches = 0;

        private async void TestTriggerAsync(object sender, ElapsedEventArgs e)
        {
            if (i == 1000)
            {
                Console.WriteLine(failedMatches);
            }
            
            if (Database.GetHistoricalCollections().HasIndex(i))
            {
                foreach (VehiclePosition position in await Database.GetHistoricalCollections().GetPositionAsync(i))
                {
                    if (!VehicleProcessingQueues.ContainsKey(position.Vehicle.ID))
                    {
                        bool result = VehicleProcessingQueues.TryAdd(position.Vehicle.ID, new BlockingCollection<SyncromaticsVehicle>());

                        if (result)
                        {
                            Task task = new Task(() => HandleVehicle(position.Vehicle.ID));
                            VehicleWorkers.Add(task);
                            task.Start();
                        }
                    }

                    VehicleProcessingQueues[position.Vehicle.ID].Add(position.Vehicle);

                    //await Syncromatics_NewVehicleDownloadedAsync(this, position.Args);
                }
            }
            i++;
        }

        private void HandleVehicle(int vehicleID)
        {
            Console.WriteLine($"Worker for vehicle {vehicleID} opened.");

            foreach(var workitem in VehicleProcessingQueues[vehicleID].GetConsumingEnumerable())
            {
                if (VehicleStates.ContainsKey(workitem.ID)) HandleExistingVehicle(workitem);
                else HandleNewVehicle(workitem);
            }

            Console.WriteLine("worker for vehicle " + vehicleID + " closed");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        private void HandleNewVehicle(SyncromaticsVehicle vehicle)
        {
            Console.WriteLine("new vehicle " + vehicle.Name);
            VehicleState state = new VehicleState(vehicle);
            Route route = Routes[vehicle.RouteID];

            /*
            TripHistory history = new TripHistory();
            history.RouteID = vehicle.RouteID;
            history.TimeLeftOrigin = DateTime.UnixEpoch;
            InProgressHistories.Add(vehicle.ID, history);
            */
            
            bool reslt = VehicleStates.TryAdd(vehicle.ID, state);
            if (!reslt) Console.WriteLine("Not added!!!!!!");

            //we have not seen this vehicle before. let's check where it is
            //StopPath stopPath = SpatialMatcher.GetStopPath(route, state);

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
                Console.WriteLine("route not the same");
                VehicleState removedState;
                VehicleStates.Remove(vehicle.ID, out removedState);
                return;
            }
            state.AddVehicleReport(vehicle);

            Route route = Routes[vehicle.RouteID];

            state.CurrentStopPath = SpatialMatcher.PolygonMatch(state, route);
            
            if (state.CurrentStopPath == null)
            {
                Console.WriteLine("Cannot match to stop path!");
                state.OnRoute = false;
                failedMatches++;
            }
            /*
            else if (state.CurrentStopPath != null)
            {
                state.OnRoute = true;

                double headway = HeadwayGenerator.CalculateHeadwayDifference(VehicleStates.Values.ToList(), route, vehicle.ID);
                bool isOnBreak = BreakGenerator.IsOnBreak(state, route);
            }
            */

            await AVLProcessing.GetWebsockets().SendVehicleUpdateAsync(new WebSockets.WSVehicleUpdateMsg(state, state.CurrentStopPath, route));
        }
    }
}
