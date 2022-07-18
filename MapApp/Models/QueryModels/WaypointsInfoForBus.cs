using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.QueryModels
{
    public class WaypointsInfoForBus
    {
        public string City { get; set; }
        public string CityToArrivalTime { get; set; }
        public string CityFromDepartTime { get; set; }
        public float Latitude { get; set; }
        public float Longtitude { get; set; }
    }
}
