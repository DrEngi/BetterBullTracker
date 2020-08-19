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

        public int StopIndex;
        public int TripIndex;

        private List<SyncromaticsVehicle> VehicleReports;

        public VehicleState(SyncromaticsVehicle vehicleReport)
        {
            VehicleReports = new List<SyncromaticsVehicle>();
            VehicleReports.Add(vehicleReport);

            BusNumber = int.Parse(vehicleReport.Name);
            Capacity = vehicleReport.APCPercentage;

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
    }
}
