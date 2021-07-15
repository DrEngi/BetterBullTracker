using BetterBullTracker.Databases.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases
{
    public class HistoricalCollections
    {
        private IMongoCollection<VehiclePosition> VehiclesCollection;
        private IMongoCollection<SyncromaticsRoute> RoutesCollection;

        Dictionary<int, List<VehiclePosition>> AllPositions = new Dictionary<int, List<VehiclePosition>>();

        public HistoricalCollections(IMongoDatabase database)
        {
            VehiclesCollection = database.GetCollection<VehiclePosition>("vehicles");
            RoutesCollection = database.GetCollection<SyncromaticsRoute>("routes");

            List<VehiclePosition> pos = VehiclesCollection.Find(x => x.Index <= 1000).ToList();
            pos.ForEach(x =>
            {
                if (AllPositions.ContainsKey(x.Index)) AllPositions[x.Index].Add(x);
                else AllPositions.Add(x.Index, new List<VehiclePosition>() { x });
            });
        }

        public bool HasIndex(int i)
        {
            return AllPositions.ContainsKey(i);
        }

        public async Task<List<VehiclePosition>> GetPositionAsync(int i)
        {
            return AllPositions[i].Where(x=> x.Vehicle.Name == "1327").ToList();
            
            /*
            
            var filter = Builders<VehiclePosition>.Filter.Eq(x => x.Index, i);
            var filter2 = Builders<VehiclePosition>.Filter.Eq(x => x.Args.Route.ShortName, "D");
            var combinedFilter = Builders<VehiclePosition>.Filter.And(filter, filter2);

            List<VehiclePosition> positions = await Collection.Find(combinedFilter).ToListAsync();
            return positions;

            */
        }

        public async Task<List<SyncromaticsRoute>> GetRoutes()
        {
            return await RoutesCollection.Find(x => true).Project<SyncromaticsRoute>("{_id: 0}").ToListAsync();
        }
    }
}
