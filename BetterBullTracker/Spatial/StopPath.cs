﻿using BetterBullTracker.Spatial.Geometry;
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
        public List<Polygon> Polygons;

        public double TotalPathDistance;
    }
}
