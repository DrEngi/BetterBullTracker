using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Databases;
using BetterBullTracker.Models.Syncromatics;
using BetterBullTracker.Spatial;
using BetterBullTracker.WebSockets;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Timers;

namespace BetterBullTracker.AVLProcessing
{
    public class SyncromaticsService
    {
        private string URL = "https://usfbullrunner.com";

        private DatabaseService Database;
        private WebsocketService Websockets;

        private Dictionary<int, Route> Routes;

        private RouteProcessor RouteProcessor;
        private VehicleProcessor VehicleProcessor;

        public SyncromaticsService(DatabaseService database, WebsocketService websockets)
        {
            Database = database;
            Websockets = websockets;

            Routes = new Dictionary<int, Route>();

            RouteProcessor = new RouteProcessor(this);
            Routes = RouteProcessor.DownloadCurrentRoutes().Result;

            VehicleProcessor = new VehicleProcessor(this, Routes);
            //VehicleProcessor.Start();
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

        public string GetURL()
        {
            return URL;
        }
    }
}
