using MapApp.Models.EF.Entities;
using System;
using System.Collections.Generic;

namespace MapApp.Models.QueryModels
{
    public class UserOrderQueryModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int AdultsCount { get; set; }
        public int Children { get; set; }
        public DateTime DepartDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public List<Service> Services { get; set; }
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan FromCityTime { get; set; }
        public TimeSpan ToCityTime { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string OrderStatus { get; set; }
        public decimal SumPrice { get; set; }
        public List<WaypointUserOrderQueryModel> Waypoints { get; set; }
        public int SeatNumber { get; set; }



        public UserOrderQueryModel()
        {
            Waypoints = new List<WaypointUserOrderQueryModel>();
            Services = new List<Service>();
        }
    }

    public class WaypointUserOrderQueryModel
    {
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public TimeSpan FromCityTime { get; set; }
        public TimeSpan ToCityTime { get; set; }
    }
}
