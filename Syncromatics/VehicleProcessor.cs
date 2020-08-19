using BetterBullTracker.Models;
using BetterBullTracker.Models.HistoricalRecords;
using BetterBullTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Syncromatics
{
    public class VehicleProcessor
    {
        SyncromaticsService Service;

        private Dictionary<int, VehicleState> VehicleStates;
        private Dictionary<int, TripHistory> InProgressHistories;

        public VehicleProcessor(SyncromaticsService service)
        {
            Service = service;
        }
    }
}
