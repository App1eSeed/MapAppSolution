using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models
{
    public class PathQuery
    {
        public string BusId { get; set; }
        public int Sequence { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string PathId { get; set; }
        //public float Longtitude { get; set; }
        //public float Latitude { get; set; }
    }
}
