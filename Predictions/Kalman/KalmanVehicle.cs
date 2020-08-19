using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Predictions.Kalman
{
    public class KalmanVehicle
    {
        public string BusNumber;

        public KalmanVehicle(string id)
        {
            BusNumber = id;
        }
    }
}
