using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.AVLProcessing.Models.Syncromatics;
using BetterBullTracker.Mapbox;
using BetterBullTracker.Spatial;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BetterBullTracker.AVLProcessing
{
    public class RouteProcessor
    {
        private SyncromaticsService Syncromatics;

        public RouteProcessor(SyncromaticsService syncromatics)
        {
            Syncromatics = syncromatics;
        }

        public async Task<Dictionary<int, Route>> DownloadCurrentRoutes()
        {
            string URL = Syncromatics.GetURL();

            List<SyncromaticsRegion> regions = await (URL + "/Regions").GetJsonAsync<List<SyncromaticsRegion>>();
            Dictionary<int, Route> routes = new Dictionary<int, Route>();

            foreach (SyncromaticsRegion region in regions)
            {
                List<SyncromaticsRoute> syncRoutes = await $"{URL}/Region/{region.ID}/Routes".GetJsonAsync<List<SyncromaticsRoute>>();
                foreach (SyncromaticsRoute route in syncRoutes)
                {
                    List<SyncromaticsRouteWaypoint> syncWaypoints = (await $"{URL}/Route/{route.ID}/Waypoints".GetJsonAsync<List<List<SyncromaticsRouteWaypoint>>>())[0];
                    List<SyncromaticsStop> syncStops = await $"{URL}/Route/{route.ID}/Direction/0/Stops".GetJsonAsync<List<SyncromaticsStop>>();

                    Route newRoute = new Route(route);
                    newRoute.RouteWaypoints = await ParseWaypoints(syncWaypoints);
                    newRoute.MapboxMatchedWaypoints = await MapboxMatch(syncWaypoints);
                    newRoute.RouteStops = ParseStops(syncWaypoints, syncStops);

                    routes.Add(route.ID, newRoute);
                }
            }
            return routes;
        }

        private async Task<List<RouteWaypoint>> ParseWaypoints(List<SyncromaticsRouteWaypoint> syncWaypoints)
        {
            List<RouteWaypoint> waypoints = new List<RouteWaypoint>();
            foreach (SyncromaticsRouteWaypoint waypoint in syncWaypoints)
            {
                waypoints.Add(new RouteWaypoint(waypoint.Latitude, waypoint.Longitude));

            }
            return waypoints;
        }

        private async Task<List<RouteWaypoint>> MapboxMatch(List<SyncromaticsRouteWaypoint> syncWaypoints)
        {
            string url = "https://api.mapbox.com/matching/v5/mapbox/driving/";
            string coordsForMapbox = "";
            List<RouteWaypoint> waypoints = new List<RouteWaypoint>();

            for (int i = 0; i < syncWaypoints.Count; i++)
            {
                if (i % 100 == 0 && i != 0) coordsForMapbox = "";
                coordsForMapbox += $"{syncWaypoints[i].Longitude},{syncWaypoints[i].Latitude};";

                if (i % 99 == 0 && i != 0 || i == syncWaypoints.Count - 1)
                {
                    coordsForMapbox = coordsForMapbox.Trim(';');
                    string mapboxAPI = url + coordsForMapbox + "?annotations=maxspeed&overview=full&geometries=geojson&access_token=pk.eyJ1IjoiZHJlbmdpIiwiYSI6ImNrMzY1NXl4aDAxanMzaHV0Zzlkd2pnZngifQ.L6C_jKfq5UCC5PkLRmFCbQ";
                    MatchingResponse response = await mapboxAPI.GetJsonAsync<MatchingResponse>();

                    foreach (List<double> coord in response.matchings[0].geometry.coordinates)
                    {
                        waypoints.Add(new RouteWaypoint(coord[1], coord[0]));
                    }
                }
            }

            //to prevent a "gap" showing on the map, we're going to add the first waypoint again to the last, mapbox will hopefully match it
            if (waypoints[waypoints.Count - 1].Coordinate.Latitude != waypoints[0].Coordinate.Latitude || waypoints[waypoints.Count - 1].Coordinate.Longitude != waypoints[0].Coordinate.Longitude)
            {
                waypoints.Add(waypoints[0]);
            }

            return waypoints;
        }

        private List<Stop> ParseStops(List<SyncromaticsRouteWaypoint> waypoints, List<SyncromaticsStop> syncromaticsStops)
        {
            List<Stop> stops = new List<Stop>();

            foreach (SyncromaticsStop stop in syncromaticsStops)
            {
                //this block of code is responsible for determining from which direction buses would approach this stop
                int waypointIndex = waypoints.FindIndex(x => x.Latitude == stop.Latitude && x.Longitude == stop.Longitude);
                if (waypointIndex != -1)
                {
                    SyncromaticsRouteWaypoint waypoint = waypoints[waypointIndex];

                    if (waypointIndex != waypoints.Count - 1)
                    {
                        SyncromaticsRouteWaypoint nextWaypoint = waypoints[waypointIndex + 1];

                        Coordinate firstCoord = new Coordinate(waypoint.Latitude, waypoint.Longitude);
                        Coordinate secondCoord = new Coordinate(nextWaypoint.Latitude, nextWaypoint.Longitude);

                        double bearing = firstCoord.GetBearingTo(secondCoord);
                        Console.WriteLine($"Stop {stop.Name} has bearing of {bearing} to next waypoint, is {Coordinate.DegreesToCardinal(bearing)}");

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
    }
}
