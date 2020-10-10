using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Predictions.Kalman
{
    public class KalmanTripSegment
    {
        private KalmanVehicleStopDetail Origin;
        private KalmanVehicleStopDetail Destination;

        public KalmanTripSegment(KalmanVehicleStopDetail origin, KalmanVehicleStopDetail destination)
        {
            Origin = origin;
            Destination = destination;
        }

        public KalmanVehicleStopDetail GetOrigin()
        {
            return this.Origin;
        }

        public KalmanVehicleStopDetail GetDestination()
        {
            return this.Destination;
        }
    }
}
