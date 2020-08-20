using BetterBullTracker.Models.Syncromatics;
using BetterBullTracker.Models.HistoricalRecords;
using BetterBullTracker.Predictions.Kalman;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BetterBullTracker.Models;

namespace BetterBullTracker.Services
{
    public class PredictionsService
    {
        DatabaseService Database;
        
        public PredictionsService(DatabaseService database)
        {
            Database = database;
        }
        
        public async Task<long> MakePrediction(Route route, SyncromaticsVehicle vehicle, VehicleState vehicleState)
        {
            //if there is sufficent historical data, use kalman

            //Stop currentStop = Spatial.StopResolver.GetCurrentStop(route, vehicle);
            //if (currentStop == null) return -1; //TODO: Partial path finding

            //long kalmanResult = await Kalman(route, vehicleState, currentStop, route.GetStopByIndex(vehicleState.StopIndex + 1));

            //return kalmanResult;
            return 0;
        }

        private async Task<long> Kalman(Route route, VehicleState vehicle, Stop originStop, Stop destinationStop)
        {
            //TODO: eventually get alternate predictions here to compare
            double time = Database.GetTripHistoryCollection().GetCurrentTimeBucket();

            TripHistory lastTripHistoryToday = await Database.GetTripHistoryCollection().GetLastTrip(route, originStop, destinationStop);
            if (lastTripHistoryToday != null) //cannot use if there aren't any vehicles proceeding it today
            {
                List<TripHistory> previousHistories = await Database.GetTripHistoryCollection().GetTripHistories(route, originStop, destinationStop, 3, 3);

                if (previousHistories != null && previousHistories.Count > 0)
                {
                    //we have enough info, ready to try predictions
                    KalmanPrediction kalman = new KalmanPrediction();
                    Predictions.Kalman.KalmanVehicle kalmanVehicle = new KalmanVehicle(vehicle.BusNumber.ToString());

                    KalmanVehicleStopDetail kalmanOriginStop = new KalmanVehicleStopDetail(originStop, 0, kalmanVehicle);
                    List<KalmanTripSegment> kalmanSegments = new List<KalmanTripSegment>();
                    foreach(TripHistory history in previousHistories)
                    {
                        KalmanVehicleStopDetail kalmanEndStop = new KalmanVehicleStopDetail(destinationStop, history.GetTravelTime(), kalmanVehicle);
                        kalmanSegments.Add(new KalmanTripSegment(kalmanOriginStop, kalmanEndStop));
                    }

                    //time information for earlier
                    KalmanVehicleStopDetail destinationDetailFromEarlier = new KalmanVehicleStopDetail(destinationStop, lastTripHistoryToday.GetTravelTime(), kalmanVehicle);
                    KalmanTripSegment earlierSegment = new KalmanTripSegment(kalmanOriginStop, destinationDetailFromEarlier);

                    double lastError = await Database.GetKalmanErrorCollection().GetKalmanError(originStop.StopID, destinationStop.StopID, route.RouteID);
                    KalmanPredictionResult result = kalman.Predict(earlierSegment, kalmanSegments, lastError);
                    long predictionTime = (long)result.GetResult();
                    double predictionError = result.GetFilterError();
                    await Database.GetKalmanErrorCollection().InsertKalmanError(predictionError, originStop.StopID, destinationStop.StopID, route.RouteID);

                    return predictionTime;
                }
                return -2;
            }
            return -3;
        }
    }
}
