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
                //this block of code is responsible for determining from which direction buses would approach this stop
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

            /*
             * we are going to re-order these stops based on where the MSC and Lib is, since, combined, they are hubs
             * for every route.
             * 
             * if a route has both the MSC and the Library as a stop point, they generally placed at opposite ends of the route.
             * for the purposes of this interim organization, if both are present, the library will be first.
             * 
             * this will eventually be used for specific route directions
             * 
             * msc ID: 401
             * library ID: 102
             */
            List<Stop> tempStopsList = new List<Stop>();
            int LIBIndex = RouteStops.FindIndex(x => x.RTPI == 102);
            int MSCIndex = RouteStops.FindIndex(x => x.RTPI == 401);

            if (MSCIndex != -1)
            {
                //both msc and lib present here
                for (int i = MSCIndex; i < RouteStops.Count; i++) tempStopsList.Add(RouteStops[i]);
                for (int i = 0; i < MSCIndex; i++) tempStopsList.Add(RouteStops[i]);
            }
            else if (LIBIndex != -1 && MSCIndex == -1)
            {
                //lib only
                for (int i = LIBIndex; i < RouteStops.Count; i++) tempStopsList.Add(RouteStops[i]);
                for (int i = 0; i < LIBIndex; i++) tempStopsList.Add(RouteStops[i]);
            }
            else if (LIBIndex == -1 && MSCIndex != -1)
            {
                //msc only
                for (int i = MSCIndex; i < RouteStops.Count; i++) tempStopsList.Add(RouteStops[i]);
                for (int i = 0; i < MSCIndex; i++) tempStopsList.Add(RouteStops[i]);
            }
            RouteStops = tempStopsList;
        }

        public Stop GetStopByIndex(int index)
        {
            return RouteStops[index];
        }

        public int GetIndexByStopID(int id)
        {
            return RouteStops.FindIndex(x => x.StopID == id);
        }
    }

    public class RouteDirection
    {
        public int ID { get; set; }
        public string DirectionName { get; set; }
        public string Cardinality { get; set; }

        public List<Stop> DirectionStops { get; set; }
        public List<RouteWaypoint> DirectionWaypoints { get; set; }

        public RouteDirection()
        {

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

        public int RTPI { get; set; }

        public Stop(Coordinate coordinate, SyncromaticsStop stop, String direction)
        {
            Coordinate = coordinate;
            StopID = stop.ID;
            StopName = stop.Name.Split("-")[1].Trim();
            Direction = direction;
            RTPI = stop.RtpiNumber;
        }
    }
}
