namespace MapApp.Models.EF.Entities
{
    public class TransportationWaipointSeat
    {
        public string Id { get; set; }
        public string TransportationWaypointId { get; set; }
        public TransportationWaypoint TransportationWaypoint { get; set; }
        public string OrderId { get; set; }
        public Order Order { get; set; }
        public int SeatNumber { get; set; }
        public bool IsTaken { get; set; }
    }
}
