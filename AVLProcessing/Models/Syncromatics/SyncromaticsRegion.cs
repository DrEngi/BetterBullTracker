using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterBullTracker.AVLProcessing.Models.Syncromatics
{
    public class SyncromaticsRegion
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public List<SyncromaticsRoute> Routes { get; set; }
    }
}
