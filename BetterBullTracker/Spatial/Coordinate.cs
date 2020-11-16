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

        /// <summary>
        /// Calculates distance to other coordinate
        /// </summary>
        /// <param name="other"></param>
        /// <returns>distance in miles</returns>
        public double DistanceTo(Coordinate other)
        {
            var d1 = this.Latitude * (Math.PI / 180.0);
            var num1 = this.Longitude * (Math.PI / 180.0);
            var d2 = other.Latitude * (Math.PI / 180.0);
            var num2 = other.Longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
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

        public override bool Equals(object obj)
        {
            Coordinate other = (Coordinate)obj;
            return other.Latitude == this.Latitude && other.Longitude == this.Longitude;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
