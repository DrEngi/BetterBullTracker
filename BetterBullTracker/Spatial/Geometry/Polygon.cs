using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Spatial.Geometry
{
    public class Polygon
    {
        private List<Coordinate> Coordinates;
        public string Direction = "any";

        public Polygon(List<Coordinate> coordinates)
        {
            Coordinates = coordinates;
        }

        public Polygon(Coordinate x1, Coordinate y1, Coordinate y2, Coordinate x2, string direction = "any")
        {
            Coordinates = new List<Coordinate>();

            Coordinates.Add(x1);
            Coordinates.Add(y1);
            Coordinates.Add(y2);
            Coordinates.Add(x2);

            Direction = direction;
        }

        public string GeoJSON()
        {
            string geo = "{\"type\": \"Feature\",\"properties\": { },\"geometry\": {\"type\": \"Polygon\",\"coordinates\":[[ ";
            Coordinates.ForEach(coord => geo += $"[{coord.Longitude}, {coord.Latitude}],");
            geo += $"[{Coordinates[0].Longitude}, {Coordinates[0].Latitude}]]]}}}},";

            return geo;
        }

        public bool Contains(Coordinate location)
        {

            var lastPoint = Coordinates[Coordinates.Count - 1];
            var isInside = false;
            var x = location.Longitude;
            foreach (var point in Coordinates)
            {
                var x1 = lastPoint.Longitude;
                var x2 = point.Longitude;
                var dx = x2 - x1;

                if (Math.Abs(dx) > 180.0)
                {
                    // we have, most likely, just jumped the dateline (could do further validation to this effect if needed).  normalise the numbers.
                    if (x > 0)
                    {
                        while (x1 < 0)
                            x1 += 360;
                        while (x2 < 0)
                            x2 += 360;
                    }
                    else
                    {
                        while (x1 > 0)
                            x1 -= 360;
                        while (x2 > 0)
                            x2 -= 360;
                    }
                    dx = x2 - x1;
                }

                if ((x1 <= x && x2 > x) || (x1 >= x && x2 < x))
                {
                    var grad = (point.Latitude - lastPoint.Latitude) / dx;
                    var intersectAtLat = lastPoint.Latitude + ((x - x1) * grad);

                    if (intersectAtLat > location.Latitude)
                        isInside = !isInside;
                }
                lastPoint = point;
            }

            return isInside;
        }

    }
}
