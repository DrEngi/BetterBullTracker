using BetterBullTracker.Models;
using BetterBullTracker.Models.Syncromatics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Spatial
{
    /// <summary>
    /// Determines what stop a vehicle is at, or null if not
    /// </summary>
    public static class StopResolver
    {
        //TODO: MSC/Lib/Math&Eng/Greek have stupidly wide distances where buses can stop, this might miss them.

        /// <summary>
        /// Returns the stop this vehicle is at if it is within 5 meters. If none, return null
        /// </summary>
        /// <param name="route">The Route this vehicle is on</param>
        /// <param name="state">The latest VehicleState for this vehicle.</param>
        /// <param name="newVehicle">If true, does not include predicted next stop because one doesn't exist yet</param>
        /// <returns>the Stop this vehicle is at, or null if not at one.</returns>
        public static Stop GetVehicleStop(Route route, VehicleState state, bool newVehicle = false)
        {
            /*
             * we are only interested in stops which are on the correct side of the road for this direction,
             * but we will also consider the next stop in this route no matter what in case Syncromatics
             * fucks up the heading.
             */
            Coordinate vehicleLocation = new Coordinate(state.GetLatestVehicleReport().Latitude, state.GetLatestVehicleReport().Longitude);
            List<Stop> validStops = route.RouteStops.FindAll(x => 
            {
                if (newVehicle) return x.Direction.Equals(state.GetLatestVehicleReport().Heading); // vvv might not be needed
                else return x.Direction.Equals(state.GetLatestVehicleReport().Heading);// || x.StopID == route.GetStopByIndex(state.StopIndex + 1).StopID;
            });

            foreach(Stop stop in validStops)
            {
                /*
                 * checks if the vehicle is within 5 meters of the original stop. if it is, return that one.
                 * if not, check the next stop along the route. this lets stop holdover times be accurate
                 * but also ensures that we don't accidentally record the stop on the other side of the street
                 * if they are relatively close together.
                 */
                if (vehicleLocation.DistanceTo(stop.Coordinate) <= 5)
                {
                    //if (state.ID == 482) Console.WriteLine($"vehicle ID 482 is {vehicleLocation.DistanceTo(stop.Coordinate)}m from {stop.StopName}");
                    return stop;
                }
            }
            return null;
        }
    }
}