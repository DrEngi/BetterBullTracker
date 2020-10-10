using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Predictions.Kalman
{
    public class KalmanPredictionResult
    {
        private double Duration;
        private double FilterError;

        public KalmanPredictionResult(double result, double filterError)
        {
            Duration = result;
            FilterError = filterError;
        }

        public double GetResult()
        {
            return Duration;
        }

        public void SetResult(double result)
        {
            Duration = result;
        }

        public double GetFilterError()
        {
            return FilterError;
        }

        public void SetFilterError(double filterError)
        {
            FilterError = filterError;
        }
    }
}
