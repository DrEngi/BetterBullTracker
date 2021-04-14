using BetterBullTracker.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.WebSockets
{
    public class WSTestCoordMsg
    {
        public Coordinate Min { get; set; }
        public Coordinate Max { get; set; }
        public int VehicleID { get; set; }

        public WSTestCoordMsg(Coordinate min, Coordinate max, int id)
        {
            Min = min;
            Max = max;
            VehicleID = id;
        }
    }
}
