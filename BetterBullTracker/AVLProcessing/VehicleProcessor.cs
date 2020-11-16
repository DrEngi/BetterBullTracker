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
            //AVLProcessing.Syncromatics.NewVehicleDownloaded += async (s, e) => await Syncromatics_NewVehicleDownloadedAsync(s, e);
            //AVLProcessing.Syncromatics.Start();
            
            String[] folder = Directory.GetDirectories("/Users/nickn/Downloads/vehicles/");
            List<String[]> directories = new List<String[]>();

            foreach(string folderName in folder)
            {
                directories.Add(Directory.GetFiles(folderName));
            }

            Task.Run(async () =>
            {
                for (int i = 0; i < 99999; i++)
                {
                    foreach (string[] directory in directories)
                    {
                        if (i > directory.Length - 1) continue;
                        VehicleDownloadedArgs e = Newtonsoft.Json.JsonConvert.DeserializeObject<VehicleDownloadedArgs>(File.ReadAllText(directory[i]));
                        if (e.Vehicle.Name.Equals("1539")) await this.Syncromatics_NewVehicleDownloadedAsync(this, e);
                    }
                    Thread.Sleep(3000);
                }
                
            });
            
        }

        private async Task Syncromatics_NewVehicleDownloadedAsync(object sender, SyncromaticsAPI.Events.VehicleDownloadedArgs e)
        {
            Console.WriteLine("Processing vehicle " + e.Vehicle.Name);

            //if (e.Route.ID == 428)
            //{
                if (VehicleStates.ContainsKey(e.Vehicle.ID)) await HandleExistingVehicle(e.Vehicle);
                else HandleNewVehicle(e.Vehicle);
            //}
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
            Stop stop = SpatialMatcher.GetVehicleStop(route, state);

            /*
             * if this vehicle is new, we want to make sure there's no wonkiness
             * by only starting to track it if it has reached the 1st stop on its route,
             * either the msc or the library.
             */
            //if (stop == null) return; //|| stop.StopID != route.RouteStops[0].StopID) return;

            TripHistory history = new TripHistory();
            history.RouteID = vehicle.RouteID;
            //history.OriginStopID = stop.StopID;
            history.TimeLeftOrigin = DateTime.UnixEpoch;

            InProgressHistories.Add(vehicle.ID, history);
            VehicleStates.Add(vehicle.ID, state);

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
            Console.WriteLine("existing vehicle " + vehicle.Name);
            VehicleState state = VehicleStates[vehicle.ID];
            if (state.GetLatestVehicleReport().Updated.Equals(vehicle.Updated))
            {
                Console.WriteLine("not updated!");
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

            Route route = Routes[vehicle.RouteID];
            Stop stop = SpatialMatcher.GetVehicleStop(route, state);
            StopPath stopPath = SpatialMatcher.GetStopPath(route, state);

            

            Console.WriteLine("sending message");
            await AVLProcessing.GetWebsockets().SendVehicleUpdateAsync(new WebSockets.WSVehicleUpdateMsg(state, stopPath));
        }
    }
}
