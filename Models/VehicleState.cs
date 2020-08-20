using BetterBullTracker.Models.Syncromatics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Models
{
    public class VehicleState
    {
        public int BusNumber { get; set; }
        public double Capacity { get; set; }

        //Note: The StopIndex here is NOT comparable to the indexes in the Route Model.
        //This one only tracks how many stops the vehicle has taken in the trip.
        public int StopIndex;
        public int TripIndex;

        public int ID;

        private List<SyncromaticsVehicle> VehicleReports;

        public VehicleState(SyncromaticsVehicle vehicleReport)
        {
            VehicleReports = new List<SyncromaticsVehicle>();
            VehicleReports.Add(vehicleReport);

            BusNumber = int.Parse(vehicleReport.Name);
            Capacity = vehicleReport.APCPercentage;
            ID = vehicleReport.ID;

            StopIndex = 0;
            TripIndex = 0;
        }

        public void AddVehicleReport(SyncromaticsVehicle vehicle)
        {
            VehicleReports.Add(vehicle);
        }

        public SyncromaticsVehicle GetLatestVehicleReport()
        {
            return VehicleReports[VehicleReports.Count - 1];
        }

        public void IncrementStopIndex(Route route)
        {
            if (StopIndex == route.RouteStops.Count)
            {
                TripIndex++;
                StopIndex = 0;
            }
            else StopIndex++;
        }

        public int GetStopIndex()
        {
            return StopIndex;
        }
    }
}
