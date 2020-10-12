using BetterBullTracker.AVLProcessing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.AVLProcessing.VehicleHandling
{
    public class HeadwayHandler
    {
        public static double CalculateHeadwayDifference(List<VehicleState> states, Route route, int vehicleIDOfInterest)
        {
            /**
             * to keep buses adequately spaced, they should be arranged 20 minutes (or less) apart.
             * we will assume there are always enough buses to meet this requirement.
             * 
             * buses should be evenly spaced, the distance they should be spaced is determined
             * by dividing the total route distance (calculated earlier) by the number of buses.
             */
            int vehicleCount = states.Count;
            int vehicleIndex = states.FindIndex(x => x.ID == vehicleIDOfInterest);
            double separationDistance = route.RouteDistance / vehicleCount; //in an ideal world, how far vehicles should be separated from each other.

            if (vehicleCount == 1) return 0.0;

            //TODO: see if this works, check performance
            /**
             * if (distance(A, C) + distance(B, C) == distance(A, B))
             *     return true; // C is on the line.
             * return false;    // C is not on the line.
             *
             *  or just:
             *
             *  return distance(A, C) + distance(B, C) == distance(A, B);
             *
             */
            int closestWaypoint = route.RouteWaypoints.FindIndex(x => x.Coordinate.Latitude == states[vehicleIndex].GetLatestVehicleReport().Latitude || x.Coordinate.Longitude == states[vehicleIndex].GetLatestVehicleReport().Longitude);
            return 0;
        }
    }
}
