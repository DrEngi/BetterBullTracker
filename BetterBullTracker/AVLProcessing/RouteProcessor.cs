using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Mapbox;
using BetterBullTracker.Spatial;
using Flurl.Http;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BetterBullTracker.AVLProcessing
{
    public class RouteProcessor
    {
        public async Task<Dictionary<int, Route>> ProcessRoutes(List<SyncromaticsRoute> routes)
        {
            Dictionary<int, Route> newRoutes = new Dictionary<int, Route>();
            foreach (SyncromaticsRoute route in routes)
            {
                Route newRoute = new Route(route);
                route.Waypoints = SubdivideWaypoints(route.Waypoints);
                newRoute.RouteWaypoints = await ParseWaypoints(route.Waypoints);
                //newRoute.MapboxMatchedWaypoints = await MapboxMatch(route.Waypoints);
                newRoute.RouteStops = ParseStops(route.Waypoints, route.Stops);
                newRoute.StopPaths = ParseStopPaths(route.Waypoints, route.Stops);
                newRoute.RouteDistance = CalculateTotalDistance(route.Waypoints);   

                double totalDistance = 0.0;
                int stopCount = 0;
                int wayPointCount = 0;
                foreach (StopPath paths in newRoute.StopPaths)
                {
                    totalDistance += paths.TotalPathDistance;
                    stopCount += 2;
                    wayPointCount += paths.Path.Count;
                }

                newRoutes.Add(route.ID, newRoute);
            }
            return newRoutes;
        }

        public async Task<Route> ProcessIndividualRoute(SyncromaticsRoute route)
        {
            Route newRoute = new Route(route);
            route.Waypoints = SubdivideWaypoints(route.Waypoints);
            newRoute.RouteWaypoints = await ParseWaypoints(route.Waypoints);
            //newRoute.MapboxMatchedWaypoints = await MapboxMatch(route.Waypoints);
            newRoute.RouteStops = ParseStops(route.Waypoints, route.Stops);
            newRoute.StopPaths = ParseStopPaths(route.Waypoints, route.Stops);
            newRoute.RouteDistance = CalculateTotalDistance(route.Waypoints);

            double totalDistance = 0.0;
            int stopCount = 0;
            int wayPointCount = 0;
            foreach (StopPath paths in newRoute.StopPaths)
            {
                totalDistance += paths.TotalPathDistance;
                stopCount += 2;
                wayPointCount += paths.Path.Count;
            }

            return newRoute;
        }

        private double CalculateTotalDistance(List<SyncromaticsWaypoint> waypoints)
        {
            double totalRouteDistance = 0.0;
            for (int i = 0; i < waypoints.Count; i++)
            {
                Coordinate secondPoint;
                if (i == waypoints.Count - 1 ) secondPoint = new Coordinate(waypoints[0].Latitude, waypoints[0].Longitude);
                else secondPoint = new Coordinate(waypoints[i + 1].Latitude, waypoints[i + 1].Longitude);

                Coordinate firstPoint = new Coordinate(waypoints[i].Latitude, waypoints[i].Longitude);
                
                totalRouteDistance += firstPoint.DistanceTo(secondPoint);
            }
            return totalRouteDistance;
        }

        private List<StopPath> ParseStopPaths(List<SyncromaticsWaypoint> waypoints, List<SyncromaticsStop> stops)
        {
            //this is REALLY messy but at least it works for now
            //TODO: Refactor
            
            List<StopPath> paths = new List<StopPath>();
            for (int i = 0; i < stops.Count; i++)
            {
                SyncromaticsStop stop = stops[i];
                
                //calculate stop paths
                int firstWaypointIndex = waypoints.FindIndex(x => x.Latitude == stop.Latitude && x.Longitude == stop.Longitude);
                if (i != stops.Count - 1)
                {
                    int lastWayPointIndex = waypoints.FindIndex(x => x.Latitude == stops[i + 1].Latitude && x.Longitude == stops[i + 1].Longitude);
                    List<Coordinate> coordinates = new List<Coordinate>();

                    for (int j = firstWaypointIndex; j < lastWayPointIndex + 1; j++)
                    {
                        coordinates.Add(new Coordinate(waypoints[j].Latitude, waypoints[j].Longitude));
                    }

                    double totalDistance = 0.0;
                    for (int j = 0; j < coordinates.Count; j++)
                    {
                        if (j != coordinates.Count - 1) totalDistance += coordinates[j].DistanceTo(coordinates[j + 1]);
                    }

                    StopPath path = new StopPath()
                    {
                        OriginStopID = stop.ID,
                        DestinationStopID = stops[i + 1].ID,
                        Path = coordinates,
                        TotalPathDistance = totalDistance
                    };
                    paths.Add(path);
                }
                else if (i == stops.Count - 1)
                {
                    int lastWayPointIndex = waypoints.FindIndex(x => x.Latitude == stops[0].Latitude && x.Longitude == stops[0].Longitude);
                    List<Coordinate> coordinates = new List<Coordinate>();

                    for (int j = firstWaypointIndex; j < waypoints.Count; j++)
                    {
                        coordinates.Add(new Coordinate(waypoints[j].Latitude, waypoints[j].Longitude));
                    }
                    for (int j = 0; j <= lastWayPointIndex; j++)
                    {
                        coordinates.Add(new Coordinate(waypoints[j].Latitude, waypoints[j].Longitude));
                    }

                    double totalDistance = 0.0;
                    for (int j = 0; j < coordinates.Count; j++)
                    {
                        if (j != coordinates.Count - 1) totalDistance += coordinates[j].DistanceTo(coordinates[j + 1]);
                    }

                    StopPath path = new StopPath()
                    {
                        OriginStopID = stop.ID,
                        DestinationStopID = stops[0].ID,
                        Path = coordinates,
                        TotalPathDistance = totalDistance
                    };
                    paths.Add(path);
                }
            }
            return paths;
        }

        private List<SyncromaticsWaypoint> SubdivideWaypoints(List<SyncromaticsWaypoint> syncWaypoints)
        {
            List<SyncromaticsWaypoint> newWaypoints = new List<SyncromaticsWaypoint>();
            for (int i = 0; i < syncWaypoints.Count; i+=2)
            {
                Coordinate firstLocation = new Coordinate(syncWaypoints[i].Latitude, syncWaypoints[i].Longitude);

                Coordinate secondLocation;
                if (i == syncWaypoints.Count - 1) secondLocation = new Coordinate(syncWaypoints[0].Latitude, syncWaypoints[0].Longitude);
                else secondLocation = new Coordinate(syncWaypoints[i + 1].Latitude, syncWaypoints[i + 1].Longitude);

                double distance = firstLocation.DistanceTo(secondLocation);
                if (distance > 20)
                {
                    double longDifference = firstLocation.Longitude - secondLocation.Longitude;
                    double latDifference = firstLocation.Latitude - secondLocation.Latitude;
                    
                    newWaypoints.Add(new SyncromaticsWaypoint()
                    {
                        Latitude = syncWaypoints[i].Latitude,
                        Longitude = syncWaypoints[i].Longitude
                    });

                    //for every 20 meters between these two waypoints, add a new one.
                    double maxInterval = (int)Math.Floor(distance / 20) + 1;
                    List<SyncromaticsWaypoint> testlist = new List<SyncromaticsWaypoint>();
                    for (double j = 1; j < maxInterval; j++)
                    {
                        double newLat = secondLocation.Latitude + (latDifference * (j / maxInterval));
                        double newLong = secondLocation.Longitude + (longDifference * (j / maxInterval));

                        testlist.Add(new SyncromaticsWaypoint()
                        {
                            Latitude = newLat,
                            Longitude = newLong
                        });

                    }
                    //reverse() required because otherwise stoppath calculation breaks due to reversed headings.
                    testlist.Reverse();
                    testlist.ForEach(x => newWaypoints.Add(x));

                    newWaypoints.Add(new SyncromaticsWaypoint()
                    {
                        Latitude = syncWaypoints[i == syncWaypoints.Count - 1 ? 0 : i + 1].Latitude,
                        Longitude = syncWaypoints[i == syncWaypoints.Count - 1 ? 0 : i + 1].Longitude
                    });
                }
                else
                {
                    newWaypoints.Add(new SyncromaticsWaypoint()
                    {
                        Latitude = syncWaypoints[i].Latitude,
                        Longitude = syncWaypoints[i].Longitude
                    });

                    //adding the last element again can sometimes cause a weird path (e.g. on route c at MSC) because it goes the wrong direction.

                    newWaypoints.Add(new SyncromaticsWaypoint()
                    {
                        Latitude = syncWaypoints[i == syncWaypoints.Count - 1 ? 0 : i + 1].Latitude,
                        Longitude = syncWaypoints[i == syncWaypoints.Count - 1 ? 0 : i + 1].Longitude
                    });
                }
            }

            return newWaypoints;
        }

        private async Task<List<RouteWaypoint>> ParseWaypoints(List<SyncromaticsWaypoint> syncWaypoints)
        {
            List<RouteWaypoint> waypoints = new List<RouteWaypoint>();
            double distance = 0;
            for (int i = 0; i < syncWaypoints.Count; i++)
            {
                SyncromaticsWaypoint waypoint = syncWaypoints[i];

                if (i != 0) distance += new Coordinate(syncWaypoints[i - 1].Latitude, syncWaypoints[i - 1].Longitude).DistanceTo(new Coordinate(waypoint.Latitude, waypoint.Longitude));
                waypoints.Add(new RouteWaypoint(waypoint.Latitude, waypoint.Longitude, distance));
            }
            
            return waypoints;
        }

        private async Task<List<RouteWaypoint>> MapboxMatch(List<SyncromaticsWaypoint> syncWaypoints)
        {
            string url = "https://api.mapbox.com/matching/v5/mapbox/driving/";
            List<RouteWaypoint> waypoints = new List<RouteWaypoint>();

            foreach(List<SyncromaticsWaypoint> sepList in SplitList<SyncromaticsWaypoint>(syncWaypoints, 99))
            {
                string coordsForMapbox = "";
                foreach(SyncromaticsWaypoint waypoint in sepList)
                {
                    coordsForMapbox += $"{waypoint.Longitude},{waypoint.Latitude};";
                }

                coordsForMapbox = coordsForMapbox.Trim(';');
                string mapboxAPI = url + coordsForMapbox + "?annotations=maxspeed&overview=full&geometries=geojson&access_token=pk.eyJ1IjoiZHJlbmdpIiwiYSI6ImNrMzY1NXl4aDAxanMzaHV0Zzlkd2pnZngifQ.L6C_jKfq5UCC5PkLRmFCbQ";
                MatchingResponse response = await mapboxAPI.GetJsonAsync<MatchingResponse>();

                double distance = 0;
                for (int i = 0; i < response.matchings[0].geometry.coordinates.Count; i++)
                {
                    List<double> coord = response.matchings[0].geometry.coordinates[i];
                    List<double> lastCoord = response.matchings[0].geometry.coordinates[i-1];

                    if (i != 0) distance += new Coordinate(lastCoord[1], lastCoord[0]).DistanceTo(new Coordinate(coord[1], coord[0]));
                    waypoints.Add(new RouteWaypoint(coord[1], coord[0], distance));
                }
            }

            //to prevent a "gap" showing on the map, we're going to add the first waypoint again to the last, mapbox will hopefully match it
            if (waypoints[waypoints.Count - 1].Coordinate.Latitude != waypoints[0].Coordinate.Latitude || waypoints[waypoints.Count - 1].Coordinate.Longitude != waypoints[0].Coordinate.Longitude)
            {
                waypoints.Add(waypoints[0]);
            }

            return waypoints;
        }

        private List<Stop> ParseStops(List<SyncromaticsWaypoint> waypoints, List<SyncromaticsStop> syncromaticsStops)
        {
            List<Stop> stops = new List<Stop>();

            for (int i = 0; i < syncromaticsStops.Count; i++)
            {
                SyncromaticsStop stop = syncromaticsStops[i];

                //calculate stop paths
                int firstWaypointIndex = waypoints.FindIndex(x => x.Latitude == stop.Latitude && x.Longitude == stop.Longitude);
                if (i != syncromaticsStops.Count - 1)
                {
                    int lastWayPointIndex = waypoints.FindIndex(x => x.Latitude == syncromaticsStops[i + 1].Latitude && x.Longitude == syncromaticsStops[i + 1].Longitude);
                    List<Coordinate> coordinates = new List<Coordinate>();

                    for (int j = firstWaypointIndex; j < lastWayPointIndex + 1; j++)
                    {
                        coordinates.Add(new Coordinate(waypoints[j].Latitude, waypoints[j].Longitude));
                    }

                    StopPath path = new StopPath()
                    {
                        OriginStopID = stop.ID,
                        DestinationStopID = syncromaticsStops[i + 1].ID,
                        Path = coordinates
                    };
                }
                

                //this block of code is responsible for determining from which direction buses would approach this stop
                if (firstWaypointIndex != -1)
                {
                    SyncromaticsWaypoint waypoint = waypoints[firstWaypointIndex];

                    if (firstWaypointIndex != waypoints.Count - 1)
                    {
                        SyncromaticsWaypoint nextWaypoint = waypoints[firstWaypointIndex + 1];

                        Coordinate firstCoord = new Coordinate(waypoint.Latitude, waypoint.Longitude);
                        Coordinate secondCoord = new Coordinate(nextWaypoint.Latitude, nextWaypoint.Longitude);

                        double bearing = firstCoord.GetBearingTo(secondCoord);

                        stops.Add(new Stop(firstCoord, stop, Coordinate.DegreesToCardinal(bearing)));
                    }
                    else Console.WriteLine($"error: stop {stop.Name} does not have a valid NEXT waypoint!");
                }
            }

            /*
             * we are going to re-order these stops based on where the MSC and Lib is, since, combined, they are hubs
             * for every route.
             * 
             * if a route has both the MSC and the Library as a stop point, they generally placed at opposite ends of the route.
             * for the purposes of this interim organization, if both are present, the msc will be first.
             * 
             * msc ID: 401
             * library ID: 102
             */
            List<Stop> tempStopsList = new List<Stop>();
            int LIBIndex = stops.FindIndex(x => x.RTPI == 102);
            int MSCIndex = stops.FindIndex(x => x.RTPI == 401);

            if (MSCIndex != -1)
            {
                //route goes to msc at all (it's always first)
                for (int i = MSCIndex; i < stops.Count; i++) tempStopsList.Add(stops[i]);
                for (int i = 0; i < MSCIndex; i++) tempStopsList.Add(stops[i]);
            }
            else if (LIBIndex != -1)
            {
                //goes to lib only
                for (int i = LIBIndex; i < stops.Count; i++) tempStopsList.Add(stops[i]);
                for (int i = 0; i < LIBIndex; i++) tempStopsList.Add(stops[i]);
            }

            return tempStopsList;
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }
}
