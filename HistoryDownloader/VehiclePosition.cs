using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoryDownloader
{
    public class VehiclePosition
    {
        public BsonObjectId _id { get; set; }
        public int Index { get; set; }
        public SyncromaticsAPI.SyncromaticsModels.SyncromaticsVehicle Vehicle { get; set; }
    }
}
