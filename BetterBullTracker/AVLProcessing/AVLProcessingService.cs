using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Databases;
using BetterBullTracker.Spatial;
using BetterBullTracker.WebSockets;
using Flurl.Http;
using SyncromaticsAPI;
using System;
using System.Collections.Generic;
using System.Timers;

namespace BetterBullTracker.AVLProcessing
{
    public class AVLProcessingService
    {
        private DatabaseService Database;
        private WebsocketService Websockets;

        public SyncromaticsAPI.SyncromaticsAPI Syncromatics;

        private Dictionary<int, Route> Routes;

        private RouteProcessor RouteProcessor;
        private VehicleProcessor VehicleProcessor;

        public AVLProcessingService(DatabaseService database, WebsocketService websockets)
        {
            Database = database;
            Websockets = websockets;
            
            Syncromatics = new SyncromaticsAPI.SyncromaticsAPI("http://usfbullrunner.com", 3000);
            Routes = new Dictionary<int, Route>();

            RouteProcessor = new RouteProcessor();
            Routes = RouteProcessor.ProcessRoutes(Syncromatics.GetRoutesAsync().Result).Result;

            VehicleProcessor = new VehicleProcessor(this, Routes);
            VehicleProcessor.Start();
        }

        public WebsocketService GetWebsockets()
        {
            return Websockets;
        }

        public DatabaseService GetDatabase()
        {
            return Database;
        }

        public Dictionary<int, Route> GetRoutes()
        {
            return Routes;
        }
    }
}
