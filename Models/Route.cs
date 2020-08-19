using BetterBullTracker.Models.Syncromatics;
using BetterBullTracker.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Models
{
    public class Route
    {
        public string RouteLetter { get; set; }
        public string RouteName { get; set; }
        public int RouteID { get; set; }

        public List<RouteWaypoint> RouteWaypoints { get; set; }
        public List<Stop> RouteStops { get; set; }
        
        public Route(SyncromaticsRoute route)
        {
            RouteLetter = route.ShortName;
            RouteName = route.Name;
            RouteID = route.ID;

            RouteWaypoints = new List<RouteWaypoint>();
            RouteStops = new List<Stop>();

            foreach(SyncromaticsRouteWaypoint waypoint in route.Waypoints[0])
            {
                RouteWaypoints.Add(new RouteWaypoint(waypoint.Latitude, waypoint.Longitude));
            }

            foreach (SyncromaticsStop stop in route.Directions[0].Stops)
            {
                int waypointIndex = route.Waypoints[0].FindIndex(x => x.Latitude == stop.Latitude && x.Longitude == stop.Longitude);
                if (waypointIndex != -1)
                {
                    SyncromaticsRouteWaypoint waypoint = route.Waypoints[0][waypointIndex];

                    if (waypointIndex != route.Waypoints[0].Count - 1)
                    {
                        SyncromaticsRouteWaypoint nextWaypoint = route.Waypoints[0][waypointIndex + 1];

                        Coordinate firstCoord = new Coordinate(waypoint.Latitude, waypoint.Longitude);
                        Coordinate secondCoord = new Coordinate(nextWaypoint.Latitude, nextWaypoint.Longitude);

                        double bearing = firstCoord.GetBearingTo(secondCoord);
                        Console.WriteLine($"Stop {stop.Name} has bearing of {bearing} to next waypoint, is {Coordinate.DegreesToCardinal(bearing)}");

                        RouteStops.Add(new Stop(firstCoord, stop, Coordinate.DegreesToCardinal(bearing)));
                    }
                    else Console.WriteLine($"error: stop {stop.Name} does not have a valid NEXT waypoint!");
                }
            }
        }

        public Stop GetStopByIndex(int index)
        {
            return RouteStops[index];
        }
    }

    public class RouteWaypoint
    {
        public Coordinate Coordinate { get; set; }

        public RouteWaypoint(double latitude, double longitude)
        {
            Coordinate = new Coordinate(latitude, longitude);
        }

        public RouteWaypoint(Coordinate coordinate)
        {
            Coordinate = coordinate;
        }
    }

    public class Stop
    {
        public Coordinate Coordinate { get; set; }

        public int StopID { get; set; }
        public string StopName { get; set; }
        public String Direction { get; set; }

        public Stop(Coordinate coordinate, SyncromaticsStop stop, String direction)
        {
            Coordinate = coordinate;
            StopID = stop.ID;
            StopName = stop.Name;
            Direction = direction;
        }
    }
}
