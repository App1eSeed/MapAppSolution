using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MapApp.Models.EF.Entities
{
    public class Transportation
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string BusId { get; set; }
        public Bus Bus { get; set; }
        [Required]
        public DateTime DepartDate { get; set; }
        [Required]
        public DateTime ArrivalDate { get; set; }
        [Required]
        public TimeSpan DepartTime { get; set; }
        [Required]
        public TimeSpan ArrivalTime { get; set; }
        public List<TransportationWaypoint> TransportationWaypoints { get; set; }
        public List<Order> Orders { get; set; }

        public Transportation()
        {        
            Orders = new List<Order>();
            TransportationWaypoints = new List<TransportationWaypoint>();
        }
    }
}
