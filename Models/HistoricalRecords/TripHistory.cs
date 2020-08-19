using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetterBullTracker.Models.Syncromatics;

namespace BetterBullTracker.Models.HistoricalRecords
{
    public class TripHistory
    {
        public double TimeBucket { get; set; }
        public int RouteID { get; set; }

        public int OriginStopID { get; set; }
        public int DestinationStopID { get; set; }

        public DateTime TimeLeftOrigin { get; set; }
        public DateTime TimeArrivedDestination { get; set; }

        public long GetTravelTime()
        {
            return (long)(TimeArrivedDestination - TimeLeftOrigin).TotalSeconds;
        }
    }
}
