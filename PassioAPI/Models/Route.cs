using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassioAPI.Models
{
    public class Route
    {
        public string name { get; set; }
        public string shortName { get; set; }
        public string color { get; set; }
        public string userId { get; set; }
        public string myid { get; set; }
        public string mapApp { get; set; }
        public string archive { get; set; }
        public string goPrefixRouteName { get; set; }
        public int goShowSchedule { get; set; }
        public string outdated { get; set; }
        public string id { get; set; }
        public int distance { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string timezone { get; set; }
        public string fullname { get; set; }
        public string nameOrig { get; set; }
    }
}
