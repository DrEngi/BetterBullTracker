﻿using BetterBullTracker.AVLProcessing.Models;
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

        }


        public static StopPath GetStopPath(Route route, VehicleState state)
        {
            SyncromaticsVehicle report = state.GetLatestVehicleReport();
            Coordinate vehicleLocation = new Coordinate(report.Latitude, report.Longitude);

            double minimum = Double.MaxValue;
            double superMinimum = 100;//furthest distance in meters that a coordinate should be away from the vehicle to be located.
            StopPath selectedPath = null;

            List<StopPath> paths = route.StopPaths.ToList();
            paths.Reverse();
            foreach(StopPath x in paths)
            {
                for (int i = 0; i < x.Path.Count - 1; i += 2)
                {
                    Coordinate firstCoord = new Coordinate(x.Path[i].Latitude, x.Path[i].Longitude);
                    Coordinate secondCoord = new Coordinate(x.Path[i + 1].Latitude, x.Path[i + 1].Longitude);

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
    }
}