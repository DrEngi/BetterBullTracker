﻿using BetterBullTracker.AVLProcessing.Models;
using BetterBullTracker.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.WebSockets
{
    public class WSVehicleUpdateMsg
    {
        public int VehicleNumber { get; set; }
        public Coordinate VehicleLocation { get; set; }

        public int PAXCount { get; set; }
        public double PAXPercentage { get; set; }

        public int RouteID { get; set; }
        public int StopIndex { get; set; }

        public WSVehicleUpdateMsg(VehicleState state)
        {
            VehicleNumber = state.BusNumber;
            VehicleLocation = new Coordinate(state.GetLatestVehicleReport().Latitude, state.GetLatestVehicleReport().Longitude);
            PAXCount = (int)Math.Floor(state.Capacity * 9);
            PAXPercentage = state.Capacity;
            RouteID = state.RouteID;
            StopIndex = state.StopIndex;
        }
    }
}