using Microsoft.AspNetCore.Routing.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases.Models
{
    public class KalmanError
    {
        public double Error { get; set; }
        public int OriginStopID { get; set; }
        public int DestinationStopID { get; set; }
        public int RouteID { get; set; }
        public DateTime Date { get; set; }
    }
}
