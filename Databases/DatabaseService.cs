using BetterBullTracker.Services.Databases;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Services
{
    public class DatabaseService
    {
        private MongoClient Client;
        private IMongoDatabase Database;

        private TripHistoryCollection TripHistory;
        private KalmanErrorCollection KalmanError;
        
        public DatabaseService()
        {
            Client = new MongoClient("");
            Database = Client.GetDatabase("bus-dev");

            TripHistory = new TripHistoryCollection(Database);
            KalmanError = new KalmanErrorCollection(Database);
        }

        public TripHistoryCollection GetTripHistoryCollection()
        {
            return TripHistory;
        }

        public KalmanErrorCollection GetKalmanErrorCollection()
        {
            return KalmanError;
        }
    }
}
