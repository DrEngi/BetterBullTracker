using SyncromaticsAPI.Events;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;

namespace SyncromaticsAPI
{
    /// <summary>
    /// Main Syncromatics API File
    /// </summary>
    public class SyncromaticsAPI
    {
        public event EventHandler<VehicleDownloadedArgs> NewVehicleDownloaded;
        
        private string BackendURL;
        private int PollRate;

        private RouteManager RouteManager;
        private VehicleManager VehicleManager;
        
        public SyncromaticsAPI(string backendURL, int pollRate)
        {
            BackendURL = backendURL;
            PollRate = pollRate;

            RouteManager = new RouteManager(BackendURL);
            VehicleManager = new VehicleManager(backendURL, pollRate, this);
        }

        /// <summary>
        /// Get routes by region, or all routes if no region is specified
        /// </summary>
        /// <param name="region"></param>
        public async System.Threading.Tasks.Task<List<SyncromaticsRoute>> GetRoutesAsync(int region = -1)
        {
            await RouteManager.DownloadAllRegions();
            List<SyncromaticsRoute> routes =  await RouteManager.DownloadAllRoutes();
            VehicleManager.SetRoutes(routes);
            return routes;
        }

        public void Start()
        {
            VehicleManager.Start();
        }

        int index = 1;
        public int getIndex()
        {
            return index;
        }
        
        internal void TriggerNewVehicleDownloaded(SyncromaticsRoute route, SyncromaticsVehicle vehicle, int i)
        {
            if (NewVehicleDownloaded != null)
            {
                index = i;
                NewVehicleDownloaded.Invoke(this, new VehicleDownloadedArgs()
                {
                    Route = route,
                    Vehicle = vehicle
                });
            }
        }
    }
}
