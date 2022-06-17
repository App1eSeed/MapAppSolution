using System;

namespace MapApp
{
    public class Route
    {
        public Route()
        {
        }

        public string BusId { get; set; }
        public string CityFrom { get; set; }
        public string CityTo { get; set; }
        public int Sequence { get; set; }
        public TimeSpan DepartTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public DateTime DepartDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public decimal Price { get; set; }
    }
}