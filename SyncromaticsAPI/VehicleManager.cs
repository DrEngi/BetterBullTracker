﻿using Flurl.Http;
using SyncromaticsAPI.Events;
using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace SyncromaticsAPI
{
    public class VehicleManager
    {
        private string BackendURL;
        private int PollRate;

        private SyncromaticsAPI MainAPI;

        private List<SyncromaticsRoute> Routes;
        private Timer Timer;

        public VehicleManager(string url, int rate, SyncromaticsAPI api)
        {
            BackendURL = url;
            PollRate = rate;
            MainAPI = api;
        }

        public void SetRoutes(List<SyncromaticsRoute> routes)
        {
            Routes = routes;
        }

        public void Start()
        {
            if (Routes != null)
            {
                Timer = new Timer(PollRate);
                Timer.AutoReset = true;
                Timer.Elapsed += new ElapsedEventHandler(DownloadVehicles);
                Timer.Start();
            }
        }

        private async void DownloadVehicles(object source, ElapsedEventArgs e)
        {
            foreach (SyncromaticsRoute route in Routes)
            {
                List<SyncromaticsVehicle> vehicles = await $"{BackendURL}/Route/{route.ID}/Vehicles".GetJsonAsync<List<SyncromaticsVehicle>>();
                foreach (SyncromaticsVehicle vehicle in vehicles)
                {
                    MainAPI.TriggerNewVehicleDownloaded(route, vehicle);
                }
            }
        }
    }
}