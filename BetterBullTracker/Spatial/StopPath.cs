using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Spatial
{
    public class StopPath
    {
        public int OriginStopID;
        public int DestinationStopID;

        public List<Coordinate> Path;
        public double TotalPathDistance;

        /// <summary>
        /// Gets the distance from the closest Point to this coordinate to the end of the stoppath
        /// </summary>
        /// <param name="startingCoord"></param>
        /// <returns></returns>
        public double GetPartialDistance(Coordinate startingCoord)
        {
            int index = 0;
            double min = Double.MaxValue;

            for(int i = 0; i < Path.Count; i++)
            {
                Coordinate tempCoord = Path[i];
                if (startingCoord.DistanceTo(tempCoord) < min)
                {
                    index = i;
                    min = startingCoord.DistanceTo(tempCoord);
                }
            }
            
            for(int i = 0; i < index; i++)
            {

            }

            return 0;
        }

    }
}
