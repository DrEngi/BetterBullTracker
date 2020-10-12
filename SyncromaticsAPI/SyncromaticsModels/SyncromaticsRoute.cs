using System;
using System.Collections.Generic;
using System.Text;

namespace SyncromaticsAPI.SyncromaticsModels
{
    public class SyncromaticsRoute
    {
        public int ID { get; set; }
        public string DisplayName { get; set; }
        public string Color { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }

        public List<SyncromaticsWaypoint> Waypoints;
        public List<SyncromaticsStop> Stops;
    }
}
