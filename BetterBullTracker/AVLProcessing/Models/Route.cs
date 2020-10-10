using BetterBullTracker.AVLProcessing.Models.Syncromatics;
using BetterBullTracker.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.AVLProcessing.Models
{
    public class Route
    {
        public string RouteLetter { get; set; }
        public string RouteName { get; set; }
        public string Color { get; set; }
        public int RouteID { get; set; }

        public List<RouteWaypoint> RouteWaypoints { get; set; }
        public List<RouteWaypoint> MapboxMatchedWaypoints { get; set; }
        public List<Stop> RouteStops { get; set; }

        public Route(SyncromaticsRoute route)
        {
            RouteLetter = route.ShortName;
            RouteName = route.Name;
            RouteID = route.ID;
            Color = route.Color;

            RouteWaypoints = new List<RouteWaypoint>();
            MapboxMatchedWaypoints = new List<RouteWaypoint>();
            RouteStops = new List<Stop>();
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
        public int RTPI { get; set; }
        public string StopName { get; set; }
        public string Direction { get; set; }

        public Stop(Coordinate coordinate, SyncromaticsStop stop, string direction)
        {
            Coordinate = coordinate;
            StopID = stop.ID;
            StopName = stop.Name.Split("-")[1].Trim();
            Direction = direction;
            RTPI = stop.RtpiNumber;
        }
    }
}
