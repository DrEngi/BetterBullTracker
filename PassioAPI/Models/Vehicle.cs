using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassioAPI.Models
{
    public class Vehicle
    {
        public int deviceId { get; set; }
        public string created { get; set; }
        public string createdTime { get; set; }
        public int paxLoad { get; set; }
        public string bus { get; set; }
        public int busId { get; set; }
        public string userId { get; set; }
        public string routeBlockId { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string calculatedCourse { get; set; }
        public int outOfService { get; set; }
        public string more { get; set; }
        public string createdDebug { get; set; }
        public int totalCap { get; set; }
        public string color { get; set; }
        public string busName { get; set; }
        public string busType { get; set; }
        public string routeId { get; set; }
        public string route { get; set; }
        public int outdated { get; set; }
    }
}
