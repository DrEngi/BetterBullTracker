using BetterBullTracker.Databases.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases
{
    public class KalmanErrorCollection
    {
        private IMongoCollection<KalmanError> Collection;

        public KalmanErrorCollection(IMongoDatabase database)
        {
            Collection = database.GetCollection<KalmanError>("kalman-error");
        }

        public async Task InsertKalmanError(double error, int originStopID, int destinationStopID, int routeID)
        {
            var routeFilter = Builders<KalmanError>.Filter.Eq(x => x.RouteID, routeID);
            var originFilter = Builders<KalmanError>.Filter.Eq(x => x.OriginStopID, originStopID);
            var destinationFilter = Builders<KalmanError>.Filter.Eq(x => x.DestinationStopID, destinationStopID);
            var combinedFilter = Builders<KalmanError>.Filter.And(routeFilter, originFilter, destinationFilter);

            var update = Builders<KalmanError>.Update.Set(x => x.Error, error);

            FindOneAndUpdateOptions<KalmanError> options = new FindOneAndUpdateOptions<KalmanError>()
            {
                IsUpsert = true
            };

            await Collection.FindOneAndUpdateAsync(combinedFilter, update, options);
        }

        public async Task<double> GetKalmanError(int originStopID, int destinationStopID, int routeID)
        {
            var routeFilter = Builders<KalmanError>.Filter.Eq(x => x.RouteID, routeID);
            var originFilter = Builders<KalmanError>.Filter.Eq(x => x.OriginStopID, originStopID);
            var destinationFilter = Builders<KalmanError>.Filter.Eq(x => x.DestinationStopID, destinationStopID);

            var combinedFilter = Builders<KalmanError>.Filter.And(routeFilter, originFilter, destinationFilter);

            KalmanError error = await Collection.Find(combinedFilter).FirstAsync();

            if (error == null) return 30;
            else return error.Error;
        }
    }
}
