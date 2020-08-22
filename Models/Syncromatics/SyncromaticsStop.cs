using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Models.Syncromatics
{
    public class SyncromaticsStop
    {
        public int ID { get; set; }
        public int RtpiNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
    }
}
