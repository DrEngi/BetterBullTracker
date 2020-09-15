using BetterBullTracker.Models.Syncromatics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.AVLProcessing.Models.Syncromatics
{
    public class SyncromaticsRoute
    {
        public int ID { get; set; }
        public string DisplayName { get; set; }
        public string Color { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }

        public List<SyncromaticsStop> Stops { get; set; }
        public List<List<SyncromaticsRouteWaypoint>> Waypoints { get; set; } //a list of a list because syncromatics?
        public List<SyncromaticsVehicle> Vehicles { get; set; }

        public SyncromaticsStop FindNextStop(SyncromaticsStop currentStop)
        {
            int index = Stops.FindIndex(x => x.ID == currentStop.ID) + 1;
            if (index > Stops.Count - 1) index = 0;
            return Stops[index];
        }
    }

    public class SyncromaticsRouteWaypoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
