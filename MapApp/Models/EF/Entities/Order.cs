using System;
using System.Collections.Generic;

namespace MapApp.Models.EF.Entities
{
    public class Order
    {
        public string Id { get; set; }
        public string TransportationId { get; set; }
        public Transportation Transportation { get; set; }
        public string OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string UserEmail { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhone { get; set; }
        public DateTime UserBirthDate { get; set; }
        public int ChildrenCount { get; set; }
        public int AdultsCount { get; set; }
        public int SeatNumber { get; set; }
        public bool Animals { get; set; }
        public bool ExcuseOfStuff { get; set; }
        public bool Apparatus { get; set; }
        public float TotalPrice { get; set; }
    }
}
