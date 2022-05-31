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
        public List<Order> Orders { get; set; }
        public List<TransportationBusSeats> TransportationBusSeats { get; set; }
        public Transportation()
        {
            Orders = new List<Order>();
            TransportationBusSeats = new List<TransportationBusSeats>();
        }
    }
}
