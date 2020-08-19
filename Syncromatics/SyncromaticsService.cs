using BetterBullTracker.Models;
using BetterBullTracker.Models.HistoricalRecords;
using BetterBullTracker.Models.Syncromatics;
using BetterBullTracker.Spatial;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Timers;

namespace BetterBullTracker.Services
{
    public class SyncromaticsService
    {
        private string URL = "https://usfbullrunner.com";
        private DatabaseService Database;
        
        private Dictionary<int, Route> Routes;
        private Dictionary<int, VehicleState> VehicleStates;
        private Dictionary<int, TripHistory> InProgressHistories;

        private Timer timer;
        int i;

        public SyncromaticsService(DatabaseService database)
        {
            Database = database;
            VehicleStates = new Dictionary<int, VehicleState>();
            Routes = new Dictionary<int, Route>();
            InProgressHistories = new Dictionary<int, TripHistory>();
            i = 0;
        }

        public async void DownloadCurrentRoutesAndStart()
        {
            List<SyncromaticsRegion> regions = await (URL + "/Regions").GetJsonAsync<List<SyncromaticsRegion>>();
            foreach(SyncromaticsRegion region in regions)
            {
                List<SyncromaticsRoute> routes = await ($"{URL}/Region/{region.ID}/Routes").GetJsonAsync<List<SyncromaticsRoute>>();
                foreach(SyncromaticsRoute route in routes)
                {
                    route.Directions = await ($"{URL}/Route/{route.ID}/Directions").GetJsonAsync<List<SyncromaticsRouteDirection>>();
                    route.Waypoints = await ($"{URL}/Route/{route.ID}/Waypoints").GetJsonAsync<List<List<SyncromaticsRouteWaypoint>>>();

                    foreach(SyncromaticsRouteDirection direction in route.Directions)
                    {
                        direction.Stops = await ($"{URL}/Route/{route.ID}/Direction/{direction.ID}/Stops").GetJsonAsync<List<SyncromaticsStop>>();
                    }

                    Route newRoute = new Route(route);
                    this.Routes.Add(route.ID, newRoute);
                }
            }

            timer = new Timer(3000);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(PullLatest);
            timer.Start();
        }

        private async void PullLatest(Object source, ElapsedEventArgs e)
        {
            i++;
            Console.WriteLine("running vehicle data collector " + i);

            foreach(int key in Routes.Keys)
            {
                List<SyncromaticsVehicle> vehicles = await $"{URL}/Route/{key}/Vehicles".GetJsonAsync<List<SyncromaticsVehicle>>();
                foreach(SyncromaticsVehicle vehicle in vehicles)
                {
                    VehicleState state;
                    if (VehicleStates.ContainsKey(vehicle.ID))
                    {
                        state = VehicleStates[vehicle.ID];
                        state.AddVehicleReport(vehicle);
                    }
                    else
                    {
                        state = new VehicleState(vehicle);
                        VehicleStates.Add(vehicle.ID, state);
                    }
                    
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
                }
            }
        }
    }
}
