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
            Routes = new ConcurrentDictionary<int, Route>(routes);

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

        int i = 1;
        RouteProcessor processor = new RouteProcessor();
        Dictionary<int, int> lostRoute = new System.Collections.Generic.Dictionary<int, int>();
        ConcurrentDictionary<int, Route> tempRoutes = new ConcurrentDictionary<int, Route>();

        private async void TestTriggerAsync(object sender, ElapsedEventArgs e)
        {
            /*
            if (i > 100)
            {
                Console.WriteLine();
                foreach (int key in lostRoute.Keys)
                {
                    
                    Console.WriteLine($"{key}: {lostRoute[key]}");
                }
                Console.WriteLine();
            }
            */
            
            foreach(VehiclePosition position in await Database.GetPositionCollection().GetPositionAsync(i))
            {
                if (!tempRoutes.ContainsKey(position.Args.Route.ID))
                {
                    Route route = await processor.ProcessIndividualRoute(position.Args.Route);
                    tempRoutes.TryAdd(position.Args.Route.ID, route);
                }

                if (!VehicleProcessingQueues.ContainsKey(position.Args.Vehicle.ID))
                {
                    bool result = VehicleProcessingQueues.TryAdd(position.Args.Vehicle.ID, new BlockingCollection<SyncromaticsVehicle>());
                    
                    if (result)
                    {
                        Task task = new Task(() => HandleVehicle(position.Args.Vehicle.ID));
                        VehicleWorkers.Add(task);
                        task.Start();
                    }
                }

                VehicleProcessingQueues[position.Args.Vehicle.ID].Add(position.Args.Vehicle);
                
                //await Syncromatics_NewVehicleDownloadedAsync(this, position.Args);
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
            Route route = tempRoutes[vehicle.RouteID];

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
            //Console.WriteLine("existing vehicle " + vehicle.Name);
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

            if (!tempRoutes.ContainsKey(vehicle.RouteID))
            {
                Console.WriteLine($"Route not yet processed: {vehicle.RouteID}");
                return;
            }

            Route route = tempRoutes[vehicle.RouteID];
            state.CurrentStopPath = SpatialMatcher.GetStopPath(route, state);
            
            double test;
            if (state.CurrentStopPath != null) test = HeadwayGenerator.CalculateHeadwayDifference(VehicleStates.Values.ToList(), route, vehicle.ID);
            
            if (state.CurrentStopPath == null)
            {
                bool msc = SpatialMatcher.IsAtMSC(state);

                if (msc) Console.WriteLine($"Vehicle {vehicle.ID} is at MSC on route {route.RouteLetter}?");
                else
                {
                    Console.WriteLine($"Vehicle {vehicle.ID} not within route {route.RouteLetter}?");
                    state.OnRoute = false;

                    if (lostRoute.ContainsKey(vehicle.ID)) lostRoute[vehicle.ID] = lostRoute[vehicle.ID] + 1;
                    else lostRoute.Add(vehicle.ID, 1);
                }
                
            }
            else if (state.CurrentStopPath != null && !state.OnRoute) state.OnRoute = true;
            
            
            await AVLProcessing.GetWebsockets().SendVehicleUpdateAsync(new WebSockets.WSVehicleUpdateMsg(state, state.CurrentStopPath, route));
        }
    }
}
