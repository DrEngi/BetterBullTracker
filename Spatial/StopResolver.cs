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
        //determine which stops are on correct side of road?
        
        /// <summary>
        /// Get the current stop the vehicle is at, any stop along the route works
        /// </summary>
        /// <param name="route">Route the vehicle is on</param>
        /// <param name="vehicle">The vehicle</param>
        /// <returns>The Models.Stop the vehicle is at, null if not at at all</returns>
        public static Stop GetCurrentStop(Route route, SyncromaticsVehicle vehicle)
        {
            Coordinate vehicleLocation = new Coordinate(vehicle.Latitude, vehicle.Longitude);
            foreach (Stop stop in route.RouteStops)
            {
                Coordinate stopLocation = new Coordinate(stop.Coordinate.Latitude, stop.Coordinate.Longitude);
                
                if (vehicleLocation.DistanceTo(stopLocation) <= 5)
                {
                    if (vehicle.ID == 482) Console.WriteLine($"vehicle ID 482 is {vehicleLocation.DistanceTo(stopLocation)}m from {stop.StopName}");
                    return stop;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the next expected stop when the vehicle passes it, otherwise null.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="vehicle"></param>
        /// <param name="originalStop"></param>
        /// <returns></returns>
        public static Stop GetVehicleExpectedStop(Route route, SyncromaticsVehicle vehicle, VehicleState state)
        {
            Coordinate vehicleLocation = new Coordinate(vehicle.Latitude, vehicle.Longitude);
            Stop originalStop = route.GetStopByIndex(state.StopIndex);
            Stop newStop = route.GetStopByIndex(state.StopIndex + 1);

            Coordinate originalStopLocation = new Coordinate(originalStop.Coordinate.Latitude, originalStop.Coordinate.Longitude);
            Coordinate newStopLocation = new Coordinate(newStop.Coordinate.Latitude, newStop.Coordinate.Longitude);

            /*
             * checks if the vehicle is within 5 meters of the original stop. if it is, return that one.
             * if not, check the next stop along the route. this lets stop holdover times be accurate
             * but also ensures that we don't accidentally record the stop on the other side of the street
             * if they are relatively close together.
             */
            if (vehicleLocation.DistanceTo(originalStopLocation) <= 5)
            {
                if (vehicle.ID == 482) Console.WriteLine($"vehicle ID 482 is {vehicleLocation.DistanceTo(newStopLocation)}m from {newStop.StopName}");
                return newStop;
            }
            else if (vehicleLocation.DistanceTo(newStopLocation) <= 5)
            {
                if (vehicle.ID == 482) Console.WriteLine($"vehicle ID 482 is {vehicleLocation.DistanceTo(newStopLocation)}m from {newStop.StopName}");
                return newStop;
            } 
            else return null;
        }
    }
}