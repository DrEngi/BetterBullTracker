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

        Dictionary<int, List<VehiclePosition>> AllPositions = new Dictionary<int, List<VehiclePosition>>();

        public PositionCollection(IMongoDatabase database)
        {
            Collection = database.GetCollection<VehiclePosition>("positions2");
            //Collection2 = database.GetCollection<VehiclePosition>("positions");

            List<VehiclePosition> pos = Collection.Find(x => x.Index <= 600).ToList();
            pos.ForEach(x =>
            {
                if (AllPositions.ContainsKey(x.Index)) AllPositions[x.Index].Add(x);
                else AllPositions.Add(x.Index, new List<VehiclePosition>() { x });
            });
        }

        public async Task InsertPositionAsync(VehiclePosition position)
        {
            await Collection.InsertOneAsync(position);
        }

        public async Task<List<VehiclePosition>> GetPositionAsync(int i)
        {
            return AllPositions[i].Where(x => x.Args.Vehicle.ID == 1786).ToList();
            
            /*
            
            var filter = Builders<VehiclePosition>.Filter.Eq(x => x.Index, i);
            var filter2 = Builders<VehiclePosition>.Filter.Eq(x => x.Args.Route.ShortName, "D");
            var combinedFilter = Builders<VehiclePosition>.Filter.And(filter, filter2);

            List<VehiclePosition> positions = await Collection.Find(combinedFilter).ToListAsync();
            return positions;

            */
        }
    }
}
