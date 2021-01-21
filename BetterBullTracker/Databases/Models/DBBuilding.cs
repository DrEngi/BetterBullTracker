using BetterBullTracker.Spatial;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases.Models
{
    public class DBBuilding
    {
        public BsonObjectId _id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public List<Coordinate> Coords { get; set; }
        public int Popularity { get; set; }
    }
}
