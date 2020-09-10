using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Models.MapboxAPI
{
    public class MatchingResponse
    {
        public string code { get; set; }
        public List<MatchingObject> matchings { get; set; }
    }

    public class MatchingObject
    {
        public double confidence;
        public double distance;
        public double duration;
        public string weight;
        public string weight_name;
        public GeometryObject geometry;
    }

    public class GeometryObject
    {
        public List<List<double>> coordinates;
    }
}
