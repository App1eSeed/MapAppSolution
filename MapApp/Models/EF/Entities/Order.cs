using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapApp.Models.EF.Entities
{

    public enum PaymentMethod
    { 
        CashOnBoarding,
        Card

    }
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        [Required]
        public string TransportationId { get; set; }
        public Transportation Transportation { get; set; }
        [Required]
        public string UserEmail { get; set; }
        [Required]
        public string UserFirstName { get; set; }
        [Required]
        public string UserLastName { get; set; }
        [Required]
        public string UserPhone { get; set; }
        [Required]
        public DateTime UserBirthDate { get; set; }
        [Required]
        public int ChildrenCount { get; set; }
        [Required]
        public int AdultsCount { get; set; }
        [Required]
        public int SeatNumber { get; set; }
        [Required]
        public string FromCityName { get; set; }
        [Required]
        public string ToCityName { get; set; }
        [Required]     
        public decimal TotalPrice { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public List<ServicesInOrder> ServicesInOrders { get; set; }
        public List<TransportationWaipointSeat> TransportationWaipointSeats { get; set; }
        public Order()
        {
            TransportationWaipointSeats = new List<TransportationWaipointSeat>();
            ServicesInOrders = new List<ServicesInOrder>();
        }
    }
}
