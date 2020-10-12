using Flurl.Http;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SyncromaticsAPI
{
    public class RouteManager
    {
        private string URL;

        private List<SyncromaticsRegion> Regions;
        private List<SyncromaticsRoute> Routes;

        public RouteManager(string url)
        {
            URL = url;
        }

        public async Task<List<SyncromaticsRegion>> DownloadAllRegions()
        {
            Regions = await (URL + "/Regions").GetJsonAsync<List<SyncromaticsRegion>>();
            return Regions;
        }

        public async Task<List<SyncromaticsRoute>> DownloadAllRoutes()
        {
            foreach(SyncromaticsRegion region in Regions)
            {
                Routes = await $"{URL}/Region/{region.ID}/Routes".GetJsonAsync<List<SyncromaticsRoute>>();
                foreach(SyncromaticsRoute route in Routes)
                {
                    route.Waypoints = (await $"{URL}/Route/{route.ID}/Waypoints".GetJsonAsync<List<List<SyncromaticsWaypoint>>>())[0];
                    route.Stops = await $"{URL}/Route/{route.ID}/Direction/0/Stops".GetJsonAsync<List<SyncromaticsStop>>();
                }
            }
            return Routes;
        }
    }
}
