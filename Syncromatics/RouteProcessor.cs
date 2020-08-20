using BetterBullTracker.Models;
using BetterBullTracker.Models.Syncromatics;
using BetterBullTracker.Services;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Syncromatics
{
    public class RouteProcessor
    {
        private SyncromaticsService Syncromatics;
        
        public RouteProcessor(SyncromaticsService syncromatics)
        {
            Syncromatics = syncromatics;
        }

        public async Task<Dictionary<int, Route>> DownloadCurrentRoutesAndStart()
        {
            string URL = Syncromatics.GetURL();
            
            List<SyncromaticsRegion> regions = await (URL + "/Regions").GetJsonAsync<List<SyncromaticsRegion>>();
            Dictionary<int, Route> routes = new Dictionary<int, Route>();
            foreach (SyncromaticsRegion region in regions)
            {
                List<SyncromaticsRoute> syncRoutes = await ($"{URL}/Region/{region.ID}/Routes").GetJsonAsync<List<SyncromaticsRoute>>();
                foreach (SyncromaticsRoute route in syncRoutes)
                {
                    route.Directions = await ($"{URL}/Route/{route.ID}/Directions").GetJsonAsync<List<SyncromaticsRouteDirection>>();
                    route.Waypoints = await ($"{URL}/Route/{route.ID}/Waypoints").GetJsonAsync<List<List<SyncromaticsRouteWaypoint>>>();

                    foreach (SyncromaticsRouteDirection direction in route.Directions)
                    {
                        direction.Stops = await ($"{URL}/Route/{route.ID}/Direction/{direction.ID}/Stops").GetJsonAsync<List<SyncromaticsStop>>();
                    }

                    Route newRoute = new Route(route);
                    routes.Add(route.ID, newRoute);
                }
            }

            return routes;
        }
    }
}
