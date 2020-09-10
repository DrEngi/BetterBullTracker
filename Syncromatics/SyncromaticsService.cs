using BetterBullTracker.Models;
using BetterBullTracker.Models.HistoricalRecords;
using BetterBullTracker.Models.Syncromatics;
using BetterBullTracker.Spatial;
using BetterBullTracker.Syncromatics;
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

        private RouteProcessor RouteProcessor;
        private VehicleProcessor VehicleProcessor;

        public SyncromaticsService(DatabaseService database)
        {
            Database = database;
            Routes = new Dictionary<int, Route>();

            RouteProcessor = new RouteProcessor(this);
            Routes = RouteProcessor.DownloadCurrentRoutes().Result;

            VehicleProcessor = new VehicleProcessor(this, Routes);
            //VehicleProcessor.Start();
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
            return this.URL;
        }
    }
}
