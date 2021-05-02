using BetterBullTracker.Databases.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases
{
    public class DwellTimeCollection
    {
        private IMongoCollection<DwellTime> Collection;

        public DwellTimeCollection(IMongoDatabase database)
        {
            Collection = database.GetCollection<DwellTime>("dwell-time");
        }

        public async Task InsertDwellTime(DwellTime time)
        {
            await Collection.InsertOneAsync(time);
        }
    }
}
