using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Spatial.Geometry;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// Returns the stop this vehicle is at if it is within 5 meters. If none, return null.
        /// OBSOLETE: Use GetStopPath() for more accurate tracking
        /// </summary>
        /// <param name="route">The Route this vehicle is on</param>
        /// <param name="state">The latest VehicleState for this vehicle.</param>
        /// <returns>the Stop this vehicle is at, or null if not at one.</returns>
        [Obsolete("Use GetStopPath() instead")]
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

        public static bool IsAtMSC(VehicleState state)
        {
            Coordinate mscCircleLocation = new Coordinate(28.064380, -82.413931); //the (rough) location of the center of the MSC circle
            double maxDistance = 22; //the max distance from the center of the MSC circle buses can be in order to be considered "at" the msc

            SyncromaticsVehicle report = state.GetLatestVehicleReport();
            Coordinate vehicleLocation = new Coordinate(report.Latitude, report.Longitude);

            return vehicleLocation.DistanceTo(mscCircleLocation) <= maxDistance;
        }

        /// <summary>
        /// you probably know what this does
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool IsAtLaurel(VehicleState state)
        {
            List<Coordinate> LaurelCoords = new List<Coordinate>()
            {
                new Coordinate(28.066435,-82.418948),
                new Coordinate(28.066812,-82.418957),
                new Coordinate(28.06722,-82.418969),
                new Coordinate(28.067323,-82.41898),
                new Coordinate(28.067322,-82.41893),
                new Coordinate(28.06732, -82.418185),
                new Coordinate(28.067331, -82.418083),
                new Coordinate(28.067369, -82.417989),
                new Coordinate(28.067407, -82.417939),
                new Coordinate(28.067443, -82.417914),
                new Coordinate(28.067388, -82.41784),
                new Coordinate(28.067281, -82.417708),
                new Coordinate(28.067209, -82.417668),
                new Coordinate(28.06713, -82.417655),
                new Coordinate(28.067004, -82.417653),
                new Coordinate(28.066856, -82.41765),
                new Coordinate(28.066742, -82.417666),
                new Coordinate(28.06663, -82.417677),
                new Coordinate(28.066486, -82.417679),
                new Coordinate(28.066367, -82.41768),
                new Coordinate(28.06626, -82.417681),
                new Coordinate(28.066105, -82.417681),
                new Coordinate(28.065999, -82.417683),
                new Coordinate(28.065925, -82.417636),
                new Coordinate(28.065877, -82.417606)
            };

            Coordinate vehicleLocation = new Coordinate(state.GetLatestVehicleReport().Latitude, state.GetLatestVehicleReport().Longitude);

            double min = Double.MaxValue;
            foreach(Coordinate coord in LaurelCoords)
            {
                double distance = vehicleLocation.DistanceTo(coord);
                if (distance <= min) min = distance;
                if (distance <= 20) return true;
            }
            //Console.WriteLine("min distance for laurel: " + min);

            return false;
        }

        public static StopPath GetStopPath(Route route, VehicleState state)
        {
            SyncromaticsVehicle report = state.GetLatestVehicleReport();
            Coordinate vehicleLocation = new Coordinate(report.Latitude, report.Longitude);

            double minimum = Double.MaxValue;
            double superMinimum = 100;//furthest distance in meters that a coordinate should be away from the vehicle to be located.
            StopPath selectedPath = null;

            List<StopPath> paths = route.StopPaths.ToList();
            for (int i = 0; i < paths.Count; i++)
            {
                StopPath x = paths[i];

                for (int j = 0; j < x.Path.Count - 1; j += 2)
                {
                    Coordinate firstCoord = new Coordinate(x.Path[j].Latitude, x.Path[j].Longitude);
                    Coordinate secondCoord = new Coordinate(x.Path[j + 1].Latitude, x.Path[j + 1].Longitude);

                    double bearing = firstCoord.GetBearingTo(secondCoord);
                    string direction = Coordinate.DegreesToCardinal(bearing);

                    if (direction.Equals(report.Heading))
                    {
                        
                        if (secondCoord.DistanceTo(vehicleLocation) < minimum)
                        {
                            minimum = secondCoord.DistanceTo(vehicleLocation);
                            selectedPath = x;
                        }
                        if (firstCoord.DistanceTo(vehicleLocation) < minimum)
                        {
                            minimum = firstCoord.DistanceTo(vehicleLocation);
                            selectedPath = x;
                        }
                    }
                }
            }

            bool valid = false;
            double min = Double.MaxValue;
            if (selectedPath != null)
            {
                foreach (Coordinate path in selectedPath.Path)
                {
                    double distance = vehicleLocation.DistanceTo(path);
                    if (distance <= superMinimum) valid = true;
                    if (distance < min) min = distance;
                }

                if (!valid) Console.WriteLine($"No valid path was found for vehicle {state.ID}, smallest value was {min}");
            }

            if (valid) return selectedPath;
            else return null;
        }

        public static StopPath PolygonMatch(VehicleState state, Route route)
        {
            Coordinate vehicleLocation = new Coordinate(state.GetLatestVehicleReport().Latitude, state.GetLatestVehicleReport().Longitude);

            foreach(StopPath stopPath in route.StopPaths)
            {
                for (int i = 0; i < stopPath.Polygons.Count; i++)
                {
                    if (stopPath.Polygons[i].Direction.Equals(state.GetLatestVehicleReport().Heading) || stopPath.Polygons[i].Direction == "any")
                    {
                        if (stopPath.Polygons[i].Contains(vehicleLocation)) return stopPath;
                    }
                }
            }

            return null;
        }
    }

    
}