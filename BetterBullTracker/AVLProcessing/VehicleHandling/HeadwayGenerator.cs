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
        public static double CalculateHeadwayDifference(List<VehicleState> states, Route route, int vehicleIDOfInterest, StopPath stopPath, WebsocketService service)
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

            VehicleState state = states.Find(x => x.ID == vehicleIDOfInterest);
            double vehicleDistance = GetDistanceAlongShape(state, route, stopPath);

            VehicleState closestVehicle;
            double closestVehicleDistance = double.MaxValue;
            foreach(VehicleState otherVehicle in states.Where(x => x.ID != vehicleIDOfInterest && x.RouteID == route.RouteID))
            {
                double otherVehicleDistance = GetDistanceAlongShape(otherVehicle, route);
                
                if (otherVehicleDistance < closestVehicleDistance)
                {
                    closestVehicle = otherVehicle;
                    closestVehicleDistance = otherVehicleDistance;
                }
            }

            /*
            int nextCoordinateIndex = closestCoord == stopPath.Path.Count - 1 ? 0 : closestCoord + 1;
            int lastCoordinateIndex = closestCoord == 0 ? stopPath.Path.Count - 1 : closestCoord - 1;

            Coordinate closestCoordinate = new Coordinate(stopPath.Path[closestCoord].Latitude, stopPath.Path[closestCoord].Longitude);
            Coordinate nextCoordinate = new Coordinate(stopPath.Path[nextCoordinateIndex].Latitude, stopPath.Path[nextCoordinateIndex].Longitude);
            Coordinate lastCoordinate = new Coordinate(stopPath.Path[lastCoordinateIndex].Latitude, stopPath.Path[lastCoordinateIndex].Longitude);
            Coordinate pairedCoordinate;

            double distanceToLast = vehicleLocation.DistanceTo(lastCoordinate);
            double distanceToNext = vehicleLocation.DistanceTo(nextCoordinate);

            if (distanceToNext <= distanceToLast)
            {
                //closer to next
                pairedCoordinate = nextCoordinate;
            }
            else pairedCoordinate = lastCoordinate;
            */

            //service.SendCoordMessageAsync(new WSTestCoordMsg(closestCoordinate, closestCoordinate, vehicleIDOfInterest));
            

            return 0;
        }

        private static double GetDistanceAlongShape(VehicleState vehicle, Route route, StopPath stopPath = null)
        {
            if (stopPath == null) stopPath = SpatialMatcher.GetStopPath(route, vehicle);

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
