using System;
using System.Collections.Generic;
using System.Text;

namespace SyncromaticsAPI.SyncromaticsModels
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
