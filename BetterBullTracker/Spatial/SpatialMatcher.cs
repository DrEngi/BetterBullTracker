using BetterBullTracker.AVLProcessing.Models;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Spatial
{
    /// <summary>
    /// Determines where a vehicle is located
    /// </summary>
    public static class SpatialMatcher
    {
        /// <summary>
        /// Returns the stop this vehicle is at if it is within 5 meters. If none, return null
        /// </summary>
        /// <param name="route">The Route this vehicle is on</param>
        /// <param name="state">The latest VehicleState for this vehicle.</param>
        /// <returns>the Stop this vehicle is at, or null if not at one.</returns>
        public static Stop GetVehicleStop(Route route, VehicleState state)
        {
            /*
             * we are only interested in stops which are on the correct side of the road for this direction,
             * but we will also consider the next stop in this route no matter what in case Syncromatics
             * fucks up the heading.
             */
            Coordinate vehicleLocation = new Coordinate(state.GetLatestVehicleReport().Latitude, state.GetLatestVehicleReport().Longitude);
            List<Stop> validStops = route.RouteStops.FindAll(x =>
            {
                //MSC/Greek have stupidly wide distances where buses can stop.
                //we are going to ignore the heading requirements for these stops

                bool isMSC = x.RTPI == 401;
                bool isGreek = x.RTPI == 432;

                return x.Direction.Equals(state.GetLatestVehicleReport().Heading) || isMSC || isGreek;
            });

            foreach(Stop stop in validStops)
            {
                /*
                 * check the location of the vehicle from every valid stop. for non msc/lib/eng/greek stops,
                 * 7 meters should be sufficent if the bus isn't speeding. otherwise, 5 should be fine
                 */

                bool isMSC = stop.RTPI == 401;
                bool isGreek = stop.RTPI == 432;

                int distance = 5;
                if (isMSC || isGreek) distance = 7;
                    
                if (vehicleLocation.DistanceTo(stop.Coordinate) <= distance) return stop;
            }
            return null;
        }

        public static StopPath GetStopPath(Route route, VehicleState state)
        {
            SyncromaticsVehicle report = state.GetLatestVehicleReport();
            Coordinate vehicleLocation = new Coordinate(report.Latitude, report.Longitude);
            
            return route.StopPaths.Find(x =>
            {
                for (int i = 0; i < x.Path.Count-1; i+=2)
                {
                    Coordinate firstCoord = new Coordinate(x.Path[i].Latitude, x.Path[i].Longitude);
                    Coordinate secondCoord = new Coordinate(x.Path[i+1].Latitude, x.Path[i+1].Longitude);

                    double bearing = firstCoord.GetBearingTo(secondCoord);
                    string direction = Coordinate.DegreesToCardinal(bearing);

                    double minimum = Double.MaxValue;


                    if (x.Path[i].Latitude - x.Path[i + 1].Latitude < 0.005 && direction.Equals(report.Heading))
                    {
                        double minLongitude = x.Path[i].Longitude;
                        double maxLongitude;
                        if (x.Path[i + 1].Longitude < minLongitude)
                        {
                            maxLongitude = minLongitude;
                            minLongitude = x.Path[i + 1].Longitude;
                        }
                        else maxLongitude = x.Path[i + 1].Longitude;

                        if (Math.Abs(vehicleLocation.Latitude - x.Path[i].Latitude) < 0.005 && (vehicleLocation.Longitude >= minLongitude && vehicleLocation.Longitude <= maxLongitude))
                        {
                            return true;
                        }
                    }
                    else if (x.Path[i].Longitude - x.Path[i+1].Longitude < 0.005 && direction.Equals(report.Heading))
                    {
                        double minLatitude = x.Path[i].Latitude;
                        double maxLatitude;
                        if (x.Path[i + 1].Latitude < minLatitude)
                        {
                            maxLatitude = minLatitude;
                            minLatitude = x.Path[i + 1].Latitude;
                        }
                        else maxLatitude = x.Path[i + 1].Latitude;

                        if (Math.Abs(vehicleLocation.Longitude - x.Path[i].Longitude) < 0.005 && (vehicleLocation.Latitude >= minLatitude && vehicleLocation.Latitude <= maxLatitude))
                        {
                            return true;
                        }
                    }
                }
                return false;
            });
        }
    }
}