using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassioAPI.Models
{
    public class Root
    {
        public Dictionary<string, Stop> Stops { get; set; }
    }
    
    public class Stop
    {
        public string routeId { get; set; }
        public string stopId { get; set; }
        public string position { get; set; }
        public string name { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string id { get; set; }
        public string userId { get; set; }
        public int radius { get; set; }
        public string routeName { get; set; }
        public string routeShortname { get; set; }
        public int routeGroupId { get; set; }
    }
}
