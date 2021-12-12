using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models
{
    public class PathQuery
    {
        public string PathId { get; set; }
        public string CountryFrom { get; set; }
        public string CountryTo { get; set; }
        public string CityFrom { get; set; }
        public string CityTo { get; set; }
        public float Distance { get; set; }
        public int Time { get; set; }
        public float Speed { get; set; }
        public string NextDepartTime { get; set; }
        public float[] CurrentLatLng { get; set; }
        public List<float[]> PathCoords { get; set; }
        public PathQuery()
        {
            PathCoords = new List<float[]>();
        }
        //public float Longtitude { get; set; }
        //public float Latitude { get; set; }
    }
}
