using BetterBullTracker.AVLProcessing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Predictions.Kalman
{
    public class KalmanVehicleStopDetail
    {
        protected Stop Stop;
        protected long Time;
        protected KalmanVehicle Vehicle;

        public KalmanVehicleStopDetail(Stop stop, long time, KalmanVehicle vehicle)
        {
            Stop = stop;
            Time = time;
            Vehicle = vehicle;
        }

        public void SetStop(Stop stop)
        {
            Stop = stop;
        }

        public Stop GetStop()
        {
            return Stop;
        }

        public void SetTime(long time)
        {
            Time = time;
        }

        public long GetTime()
        {
            return Time;
        }

        public void SetVehicle(KalmanVehicle vehicle)
        {
            Vehicle = vehicle;
        }

        public KalmanVehicle GetVehicle()
        {
            return Vehicle;
        }
    }
}
