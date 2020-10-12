using System;
using System.Collections.Generic;
using System.Text;

namespace SyncromaticsAPI.SyncromaticsModels
{
    public class SyncromaticsVehicle
    {
        public int ID { get; set; }
        public int APCPercentage { get; set; }
        public int RouteID { get; set; }
        public string Name { get; set; }
        public int DoorStatus { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Heading { get; set; }

        public string Updated { get; set; }

        /// <summary>
        /// converts the Syncromatics Updated field into something that DateTime can use.
        /// </summary>
        /// <returns></returns>
        public string AcceptableUpdated()
        {
            bool isAM = Updated.Contains("A");

            if (isAM) return $"{Updated.Replace("A", "")} AM";
            else return $"{Updated.Replace("P", "")} PM";
        }
    }
}
