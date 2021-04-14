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
    public class HeadwayHandler
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
            Coordinate vehicleLocation = new Coordinate(state.GetLatestVehicleReport().Latitude, state.GetLatestVehicleReport().Longitude);
            
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

            service.SendCoordMessageAsync(new WSTestCoordMsg(closestCoordinate, pairedCoordinate, vehicleIDOfInterest));
            

            return 0;
        }
    }
}
