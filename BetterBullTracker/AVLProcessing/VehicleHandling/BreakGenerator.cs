using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.AVLProcessing.VehicleHandling
{
    public static class BreakGenerator
    {
        /**
         * rules for determining if a vehicle is on a break or is otherwise temporarily unavailable:
         * 1. any vehicle is off of its specified route
         * 2. any vehicle suddenly disconnects from syncromatics for more than several requests, but it was not at (or at least very close to) the final stop
         * 3. on routes a,c,d,e:
         *    a. when a vehicle is stopped for excessive time on the east side of the marshall center
         *    b. when a vehicle is stopped for excessive time on the northeast side of the library (leroy collins) [e mod only]
         * 4. on route b:
         *    a. when a vehicle is stopped for excessive time on the east side of the marshall center
         *    b. when a vehicle is stopped for excessive time at Continuing Education (NEC)
         * 5. on route f,g:
         *    a. when a vehicle is stopped for excessive time on the northeast side of the library (leroy collins)
         *    
         * dwell time is not considered here, although it is often that drivers will stop in these places to recover headway or for other reasons.
         * (in my own experience, drivers usually stop at the proper stop locations to recover headway, but the NEC (and actually all of B) might be a problem in particular)
        **/

        public static bool IsOnBreak(VehicleState state, Route route)
        {
            bool isAtMSC = false;
            bool isAtLaurel = false;
            bool isAtBase = false;

            if (route.RouteStops.Count(x => x.StopID == 95569) == 0 && SpatialMatcher.IsAtLaurel(state)) isAtLaurel = true;
            if (route.RouteStops.Count(x => x.RTPI == 401) == 0 && SpatialMatcher.IsAtMSC(state)) isAtMSC = true;

            return isAtMSC || isAtLaurel || isAtBase;
        }
    }
}
