using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Predictions.Kalman
{
    public class KalmanPrediction
    {
        public KalmanPredictionResult Predict(KalmanTripSegment lastVehicleSegment, List<KalmanTripSegment> historicalSegments, double lastError)
        {
            double average = HistoricalAverage(historicalSegments);
            double variance = HistoricalVariance(historicalSegments, average);
            double gain = Gain(average, variance, lastError);
            double loopGain = 1 - gain;

            KalmanPredictionResult result = new KalmanPredictionResult(Prediction(gain, loopGain, historicalSegments, lastVehicleSegment, average), FilterError(variance, gain));
            return result;
        }

        private double HistoricalAverage(List<KalmanTripSegment> historicalSegments)
        {
            if (historicalSegments.Count > 0)
            {
                long total = 0;
                foreach (KalmanTripSegment segment in historicalSegments)
                {
                    long duration = segment.GetDestination().GetTime() - segment.GetOrigin().GetTime();
                    total += duration;
                }
                return (double)(total / historicalSegments.Count);
            }
            else throw new Exception();
        }

        private double HistoricalVariance(List<KalmanTripSegment> historicalSegments, double average)
        {
            if (historicalSegments.Count > 0)
            {
                double total = 0;
                foreach (KalmanTripSegment segment in historicalSegments)
                {
                    long duration = segment.GetDestination().GetTime() - segment.GetOrigin().GetTime();
                    double difference = duration - average;
                    double diffSquared = difference * difference;

                    total += diffSquared;
                }
                return (double)(total / historicalSegments.Count);
            }
            else throw new Exception();
        }

        private double FilterError(double variance, double loopGain)
        {
            return variance * loopGain;
        }

        public double Gain(double average, double variance, double lastPredictionError)
        {
            return (lastPredictionError + variance) / (lastPredictionError + (2 * variance));
        }

        private double Prediction(double gain, double loopGain, List<KalmanTripSegment> historicalSegments, KalmanTripSegment lastSegment, double averageDuration)
        {
            double historicalDuration = averageDuration;

            long lastVehicleDuration = lastSegment.GetDestination().GetTime() - lastSegment.GetOrigin().GetTime();
            double prediction = (loopGain * lastVehicleDuration) + (gain * historicalDuration);

            return prediction;
        }
    }
}
