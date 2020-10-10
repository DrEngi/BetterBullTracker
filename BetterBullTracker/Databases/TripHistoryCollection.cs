using BetterBullTracker.Models.Syncromatics;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Databases.Models;

namespace BetterBullTracker.Databases
{
    /// <summary>
    /// two hour increments
    /// </summary>
    public class TripHistoryCollection
    {
        private IMongoCollection<TripHistory> Collection;
        private int TimeInterval = 120 * 60; //time buckets are separated into two hour intervals

        public TripHistoryCollection(IMongoDatabase database)
        {
            Collection = database.GetCollection<TripHistory>("trip-histories");
        }

        /// <summary>
        /// Get the current time bucket records will be stored in, or would've been on previous days.
        /// </summary>
        /// <returns>the current time bucket</returns>
        public double GetCurrentTimeBucket()
        {
            double time = (DateTime.Now - DateTime.Today).TotalSeconds; //number of seconds since midnight
            time = Math.Floor(time / TimeInterval) * TimeInterval;
            return time;
        }

        public async Task InsertTripHistory(TripHistory history)
        {
            await Collection.InsertOneAsync(history);
        }

        /// <summary>
        /// Gets the trip histories for the given number of days for the given route between the given stops, in the current time bucket
        /// </summary>
        /// <param name="route">The Route to search for</param>
        /// <param name="origin">The Stop the vehicle originates from</param>
        /// <param name="destination">The destination the vehicle heads to</param>
        /// <param name="daysPrior">The max number of days to go back for</param>
        /// <param name="limit">The max number of histories to return</param>
        /// <param name="bucket">The TimeBucket to search for on previous days. Defaults to the current one in use.</param>
        /// <returns>A List of TripHistory objects</returns>
        public async Task<List<TripHistory>> GetTripHistories(Route route, Stop origin, Stop destination, int daysPrior, int limit, double bucket = -1)
        {
            if (bucket == -1) bucket = GetCurrentTimeBucket();

            var routeFilter = Builders<TripHistory>.Filter.Eq(x => x.RouteID, route.RouteID);
            var originFilter = Builders<TripHistory>.Filter.Eq(x => x.OriginStopID, origin.StopID);
            var destinationFilter = Builders<TripHistory>.Filter.Eq(x => x.DestinationStopID, destination.StopID);
            var timeFilter = Builders<TripHistory>.Filter.Gte(x => x.TimeArrivedDestination, DateTime.Now.AddDays(-daysPrior));

            var sort = Builders<TripHistory>.Sort.Descending(x => x.TimeArrivedDestination);
            var combinedFilter = Builders<TripHistory>.Filter.And(routeFilter, originFilter, destinationFilter, timeFilter);
            return await Collection.Find(combinedFilter).Sort(sort).Limit(limit).ToListAsync();
        }

        /// <summary>
        /// Get the last trip that was concluded today (on/after 12:00 AM that morning)
        /// </summary>
        /// <param name="route"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public async Task<TripHistory> GetLastTrip(Route route, Stop origin, Stop destination)
        {
            var timeFilter = Builders<TripHistory>.Filter.Gte(x => x.TimeArrivedDestination, DateTime.Today);
            var routeFilter = Builders<TripHistory>.Filter.Eq(x => x.RouteID, route.RouteID);
            var originFilter = Builders<TripHistory>.Filter.Eq(x => x.OriginStopID, origin.StopID);
            var destinationFilter = Builders<TripHistory>.Filter.Eq(x => x.DestinationStopID, destination.StopID);

            var sort = Builders<TripHistory>.Sort.Descending(x => x.TimeArrivedDestination);
            var combinedFilter = Builders<TripHistory>.Filter.And(routeFilter, originFilter, destinationFilter, timeFilter);
            return await Collection.Find(combinedFilter).Sort(sort).Limit(1).FirstAsync();
        }
    }
}
