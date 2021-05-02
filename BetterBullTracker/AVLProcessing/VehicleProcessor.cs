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
            /*
            AVLProcessing.Syncromatics.NewVehicleDownloaded += async (s, e) => await Syncromatics_NewVehicleDownloadedAsync(s, e);
            AVLProcessing.Syncromatics.Start();
            */

            System.Timers.Timer Timer = new System.Timers.Timer(3000);
            Timer.AutoReset = true;
            Timer.Elapsed += new ElapsedEventHandler(TestTriggerAsync);
            Timer.Start();
        }

        int i = 1;
        RouteProcessor processor = new RouteProcessor();
        Dictionary<int, Route> tempRoutes = new Dictionary<int, Route>();
        private async void TestTriggerAsync(object sender, ElapsedEventArgs e)
        {
            foreach(VehiclePosition position in await Database.GetPositionCollection().GetPositionAsync(i))
            {
                if (!tempRoutes.ContainsKey(position.Args.Route.ID)) tempRoutes.Add(position.Args.Route.ID, await processor.ProcessIndividualRoute(position.Args.Route));

                await Syncromatics_NewVehicleDownloadedAsync(this, position.Args);
            }
            i++;
        }

        private async Task Syncromatics_NewVehicleDownloadedAsync(object sender, SyncromaticsAPI.Events.VehicleDownloadedArgs e)
        {
            Console.WriteLine("Processing vehicle " + e.Vehicle.Name);

            /*
            await Database.GetPositionCollection().InsertPositionAsync(new VehiclePosition()
            {
                Args = e,
                Index = AVLProcessing.Syncromatics.getIndex()
            });
            */

            
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
            Console.WriteLine("new vehicle " + vehicle.Name);
            VehicleState state = new VehicleState(vehicle);
            Route route = tempRoutes[vehicle.RouteID];

            /*
            TripHistory history = new TripHistory();
            history.RouteID = vehicle.RouteID;
            history.TimeLeftOrigin = DateTime.UnixEpoch;
            InProgressHistories.Add(vehicle.ID, history);
            */

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
            //Console.WriteLine("existing vehicle " + vehicle.Name);
            VehicleState state = VehicleStates[vehicle.ID];
            if (state.GetLatestVehicleReport().Updated.Equals(vehicle.Updated))
            {
                return; //we aren't interested in reports that haven't been updated
            }

            if (state.GetLatestVehicleReport().RouteID != vehicle.RouteID)
            {
                //if a vehicle changes routes, remove its state and restart
                Console.WriteLine("route not the same");
                VehicleStates.Remove(vehicle.ID);
                return;
            }
            state.AddVehicleReport(vehicle);

            
            Route route = tempRoutes[vehicle.RouteID];
            StopPath stopPath = SpatialMatcher.GetStopPath(route, state);
            double headway = HeadwayGenerator.CalculateHeadwayDifference(VehicleStates.Values.ToList(), route, vehicle.ID, stopPath, AVLProcessing.GetWebsockets());

            
            await AVLProcessing.GetWebsockets().SendVehicleUpdateAsync(new WebSockets.WSVehicleUpdateMsg(state, stopPath));
        }
    }
}
