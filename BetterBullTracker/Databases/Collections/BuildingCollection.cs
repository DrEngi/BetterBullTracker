using BetterBullTracker.Databases.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases
{
    public class BuildingCollection
    {
        private IMongoCollection<DBBuilding> Collection;

        public BuildingCollection(IMongoDatabase database)
        {
            Collection = database.GetCollection<DBBuilding>("buildings");
        }

        public async Task<List<DBBuilding>> GetBuildingsAsync(string name)
        {
            //^(?=.*\bMaple.*A\b)(?=.*\bMaple.*A\b).*$
            var nameFilter = Builders<DBBuilding>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(@$"(?i)^(?=.*\b{name.Replace(" ", ".*")}\b)(?=.*\b{name.Replace(" ", ".*")}\b).*"));
            var shortNameFilter = Builders<DBBuilding>.Filter.Regex(x => x.ShortName, new MongoDB.Bson.BsonRegularExpression($"(?i).*{name}.*"));
            var combinedFilter = Builders<DBBuilding>.Filter.Or(nameFilter, shortNameFilter);
            return await (await Collection.FindAsync(combinedFilter)).ToListAsync();
        }
    }
}
