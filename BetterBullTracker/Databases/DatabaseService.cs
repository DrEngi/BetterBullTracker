﻿using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases
{
    public class DatabaseService
    {
        private MongoClient Client;
        private IMongoDatabase Database;

        private TripHistoryCollection TripHistory;
        private KalmanErrorCollection KalmanError;
        private BuildingCollection Buildings;
        private PositionCollection Positions;

        public DatabaseService()
        {
            DatabaseConfig config = JsonConvert.DeserializeObject<DatabaseConfig>(File.ReadAllText("config.json"));

            Client = new MongoClient($"mongodb://{config.username}:{config.password}@{config.address}:{config.port}");
            Database = Client.GetDatabase("bus-dev");

            TripHistory = new TripHistoryCollection(Database);
            KalmanError = new KalmanErrorCollection(Database);
            Buildings = new BuildingCollection(Database);
            Positions = new PositionCollection(Database);

        }

        public TripHistoryCollection GetTripHistoryCollection()
        {
            return TripHistory;
        }

        public KalmanErrorCollection GetKalmanErrorCollection()
        {
            return KalmanError;
        }

        public BuildingCollection GetBuildingCollection()
        {
            return Buildings;
        }

        public PositionCollection GetPositionCollection()
        {
            return Positions;
        }
    }

    public class DatabaseConfig
    {
        public string address { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}
