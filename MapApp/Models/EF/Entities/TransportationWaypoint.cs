using System.Collections.Generic;

namespace MapApp.Models.EF.Entities
{
    public class TransportationWaypoint
    {
        public string Id { get; set; }
        public string TransportationId { get; set; }
        public Transportation Transportation { get; set; }
        public string WayPointsScheduleId { get; set; }
        public WayPointsSchedule WayPointsSchedule { get; set; }
        public decimal Price { get; set; }
        public List<TransportationWaipointSeat> TransportationWaipointSeats { get; set; }
        public TransportationWaypoint()
        {
            TransportationWaipointSeats = new List<TransportationWaipointSeat>();
        }
    }
}
