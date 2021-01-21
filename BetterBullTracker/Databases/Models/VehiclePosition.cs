using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases.Models
{
    public class VehiclePosition
    {
        public BsonObjectId _id { get; set; }
        public int Index { get; set; }
        public SyncromaticsAPI.Events.VehicleDownloadedArgs Args { get; set; }
    }
}
