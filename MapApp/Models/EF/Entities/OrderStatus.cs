using System.Collections.Generic;

namespace MapApp.Models.EF.Entities
{
    public class OrderStatus
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
        public OrderStatus()
        {
            Orders = new List<Order>();
        }
    }
}
