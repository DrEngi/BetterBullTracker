using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Spatial
{
    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double DistanceTo(Coordinate other)
        {
            double R = 671e3;

            double theta1 = this.Latitude * Math.PI / 180;
            double theta2 = other.Latitude * Math.PI / 180;

            double deltaTheta = (other.Latitude - this.Latitude) * Math.PI / 180;
            double deltaAlpha = (other.Longitude - this.Longitude) * Math.PI / 180;

            double a = Math.Sin(deltaTheta / 2) * Math.Sin(deltaTheta / 2) + Math.Cos(theta1) * Math.Cos(theta2) * Math.Sin(deltaAlpha / 2) * Math.Sin(deltaAlpha / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public double GetBearingTo(Coordinate other)
        {
            var dLon = ToRad(other.Longitude - this.Longitude);
            var dPhi = Math.Log(
                Math.Tan(ToRad(other.Latitude) / 2 + Math.PI / 4) / Math.Tan(ToRad(this.Latitude) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        private static double DegreeBearing(double lat1, double lon1, double lat2, double lon2)
        {
            var dLon = ToRad(lon2 - lon1);
            var dPhi = Math.Log(
                Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        private static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        private static double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }

        public static string DegreesToCardinal(double degrees)
        {
            string[] caridnals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
            return caridnals[(int)Math.Round(((double)degrees % 360) / 45)];
        }

        public override String ToString()
        {
            return $"{Latitude}, {Longitude}";
        }
    }
}
