using System.ComponentModel.DataAnnotations;

namespace MapApp.Models.EF.Entities
{
    //Можно соеденить с Path - запросы будут легче
    public class TransportationWaipointSeat
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string TransportationWaypointId { get; set; }
        public TransportationWaypoint TransportationWaypoint { get; set; }
        
        public string OrderId { get; set; }
        public Order Order { get; set; }
        [Required]
        public int SeatNumber { get; set; }
        [Required]
        public bool IsTaken { get; set; }
    }
}
