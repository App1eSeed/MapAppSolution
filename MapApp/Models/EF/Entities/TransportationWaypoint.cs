using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MapApp.Models.EF.Entities
{
    public class TransportationWaypoint
    {
        public string Id { get; set; }
        [Required]
        public string TransportationId { get; set; }
        public Transportation Transportation { get; set; }
        [Required]
        public string WayPointsScheduleId { get; set; }
        public WayPointsSchedule WayPointsSchedule { get; set; }
        [Required]
        public decimal Price { get; set; }
        public List<TransportationWaipointSeat> TransportationWaipointSeats { get; set; }
        public TransportationWaypoint()
        {
            TransportationWaipointSeats = new List<TransportationWaipointSeat>();
        }
    }
}
