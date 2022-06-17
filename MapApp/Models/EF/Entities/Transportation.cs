using System;
using System.Collections.Generic;

namespace MapApp.Models.EF.Entities
{
    public class Transportation
    {
        public string Id { get; set; }
        public string BusId { get; set; }
        public Bus Bus { get; set; }
        public DateTime DepartDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public TimeSpan DepartTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public List<TransportationWaypoint> TransportationWaypoints { get; set; }
          
        public Transportation()
        {        
          
            TransportationWaypoints = new List<TransportationWaypoint>();
        }
    }
}
