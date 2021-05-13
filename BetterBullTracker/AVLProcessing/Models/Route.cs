using BetterBullTracker.Spatial;
using Newtonsoft.Json;
using SyncromaticsAPI.SyncromaticsModels;
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
        public double RouteDistance;

        public List<RouteWaypoint> RouteWaypoints { get; set; }
        public List<RouteWaypoint> MapboxMatchedWaypoints { get; set; }
        public List<Stop> RouteStops { get; set; }
        public List<StopPath> StopPaths;

        public Route(SyncromaticsRoute route)
        {
            RouteLetter = route.ShortName;
            RouteName = route.Name;
            RouteID = route.ID;
            Color = route.Color;

            RouteWaypoints = new List<RouteWaypoint>();
            MapboxMatchedWaypoints = new List<RouteWaypoint>();
            StopPaths = new List<StopPath>();
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
        public double Distance { get; set; }

        public RouteWaypoint(double latitude, double longitude, double distance)
        {
            Coordinate = new Coordinate(latitude, longitude);
            Distance = distance;
        }

        public RouteWaypoint(Coordinate coordinate, double distance)
        {
            Coordinate = coordinate;
            Distance = distance;
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
            StopName = stop.Name.Contains("-") ? stop.Name.Split("-")[1].Trim() : stop.Name.Trim();
            Direction = direction;
            RTPI = stop.RtpiNumber;
        }
    }
}
