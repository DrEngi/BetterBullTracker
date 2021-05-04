using BetterBullTracker.Databases.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases
{
    public class PositionCollection
    {
        private IMongoCollection<VehiclePosition> Collection;
        private IMongoCollection<VehiclePosition> Collection2;

        public PositionCollection(IMongoDatabase database)
        {
            Collection = database.GetCollection<VehiclePosition>("positions2");
            //Collection2 = database.GetCollection<VehiclePosition>("positions");
        }

        public async Task InsertPositionAsync(VehiclePosition position)
        {
            await Collection.InsertOneAsync(position);
        }

        public async Task<List<VehiclePosition>> GetPositionAsync(int i)
        {
            var filter = Builders<VehiclePosition>.Filter.Eq(x => x.Index, i);
            var filter2 = Builders<VehiclePosition>.Filter.Eq(x => x.Args.Vehicle.ID, 2710);
            var combinedFilter = Builders<VehiclePosition>.Filter.And(filter, filter2);

            List<VehiclePosition> positions = await Collection.Find(combinedFilter).ToListAsync();
            return positions;
        }
    }
}
