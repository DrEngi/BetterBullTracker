using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Spatial;
using BetterBullTracker.WebSockets;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.AVLProcessing.VehicleHandling
{
    public class HeadwayGenerator
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
            int vehicleCount = states.Where(x => x.RouteID == route.RouteID).Count();
            double idealSeparationDistance = route.RouteDistance / vehicleCount; //in an ideal world, how far vehicles should be separated from each other.

            if (vehicleCount == 1) return 0.0;

            List<(double, VehicleState)> otherVehicles = new List<(double, VehicleState)>();
            foreach(VehicleState otherVehicle in states.Where(x => x.RouteID == route.RouteID))
            {
                otherVehicles.Add((GetDistanceAlongShape(otherVehicle, route), otherVehicle));
            }

            otherVehicles.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            int vehicleIndex = otherVehicles.FindIndex(x => x.Item2.ID == vehicleIDOfInterest);
            int nextIndex = vehicleIndex == otherVehicles.Count - 1 ? 0 : vehicleIndex + 1;

            double separationDistance = Math.Abs(otherVehicles[vehicleIndex].Item1 - otherVehicles[nextIndex].Item1);

            return separationDistance;
        }

        private static double GetDistanceAlongShape(VehicleState vehicle, Route route, StopPath stopPath = null)
        {
            if (stopPath == null) stopPath = SpatialMatcher.GetStopPath(route, vehicle);
            if (stopPath == null) return -1;

            Coordinate vehicleLocation = new Coordinate(vehicle.GetLatestVehicleReport().Latitude, vehicle.GetLatestVehicleReport().Longitude);

            double minimum = Double.MaxValue;
            int closestCoord = -1;
            for (int i = 0; i < stopPath.Path.Count; i++)
            {
                Coordinate coord = new Coordinate(stopPath.Path[i].Latitude, stopPath.Path[i].Longitude);
                if (coord.DistanceTo(vehicleLocation) < minimum)
                {
                    minimum = coord.DistanceTo(vehicleLocation);
                    closestCoord = i;
                }
            }

            RouteWaypoint closestWaypoint = route.RouteWaypoints.Find(x => x.Coordinate.Latitude == stopPath.Path[closestCoord].Latitude && x.Coordinate.Longitude == stopPath.Path[closestCoord].Longitude);

            return closestWaypoint.Distance;
        }
    }
}
