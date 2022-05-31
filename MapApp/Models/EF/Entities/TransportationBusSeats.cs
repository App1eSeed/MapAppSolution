namespace MapApp.Models.EF.Entities
{
    public class TransportationBusSeats
    {
        public string Id { get; set; }
        public string TransportationId { get; set; }
        public Transportation Transportation { get; set; }
        public int SeatNumber { get; set; }
        public bool IsTaken { get; set; }
    }
}
