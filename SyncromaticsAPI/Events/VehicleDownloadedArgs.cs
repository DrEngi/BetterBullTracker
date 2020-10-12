using SyncromaticsAPI.SyncromaticsModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncromaticsAPI.Events
{
    public class VehicleDownloadedArgs : EventArgs
    {
        public SyncromaticsVehicle Vehicle;
        public SyncromaticsRoute Route;
    }
}
