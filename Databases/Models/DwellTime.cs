﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.Databases.Models
{
    public class DwellTime
    {
        public DateTime Time { get; set; }
        public int RouteID { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
