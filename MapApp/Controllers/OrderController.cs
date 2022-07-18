using MapApp.Models.EF;
using MapApp.Models.EF.Entities;
using MapApp.Models.QueryModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MapApp.Controllers
{
    public class OrderController : Controller
    {
        //[Authorize]
        public IActionResult Order()
        {
            using (var context = new MapAppContext())
            {

                string userEmail = User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

                var fullUserOrderData = (from user in context.Users
                                         join order in context.Orders on user.Email equals order.UserEmail
                                         join transp in context.Transportations on order.TransportationId equals transp.Id
                                         where user.Email == userEmail
                                         select new UserOrderQueryModel()
                                         {
                                             FirstName = user.FirstName,
                                             LastName = user.LastName,
                                             Email = user.Email,
                                             Phone = user.PhoneNumber,
                                             AdultsCount = order.AdultsCount,
                                             Children = order.ChildrenCount,
                                             FromCity = order.FromCityName,
                                             ToCity = order.ToCityName,
                                             DepartDate = transp.DepartDate,
                                             ArrivalDate = transp.ArrivalDate,
                                             OrderStatus = order.OrderStatusId,
                                             SeatNumber = order.SeatNumber,
                                             PaymentMethod = order.PaymentMethod,
                                             SumPrice = order.TotalPrice,

                                            Services = (from order1 in context.Orders
                                                        join serviceInOrdeder in context.ServicesInOrders on order1.Id equals serviceInOrdeder.OrderId
                                                        join service in context.Services on serviceInOrdeder.ServiceId equals service.Id
                                                        where order1.Id == order.Id
                                                        select new Service() {
                                                            Id = service.Id,
                                                            Name = service.Name,
                                                            Price = service.Price
                                                        }).ToList(),

                                             Waypoints = (from waypoint1 in context.WayPointsSchedules
                                                          join bus1 in context.Buses on waypoint1.BusId equals bus1.Id
                                                          join transp1 in context.Transportations on bus1.Id equals transp1.BusId
                                                          join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                                          join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                                          join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                                          where waypoint1.Sequence >= (from waypoint2 in context.WayPointsSchedules
                                                                                       join path2 in context.Paths on waypoint2.PathId equals path2.Id
                                                                                       join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                                                                                       join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                                                                                       where cityFrom2.Name == order.FromCityName
                                                                                        && waypoint2.BusId == transp.BusId
                                                                                       select waypoint2.Sequence).First()
                                                             && waypoint1.Sequence <= (from waypoint2 in context.WayPointsSchedules
                                                                                       join path2 in context.Paths on waypoint2.PathId equals path2.Id
                                                                                       join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                                                                                       join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                                                                                       where cityTo2.Name == order.ToCityName
                                                                                        && waypoint2.BusId == transp.BusId
                                                                                       select waypoint2.Sequence).First()
                                                             && transp1.Id == order.TransportationId
                                                          orderby waypoint1.Sequence
                                                          select new WaypointUserOrderQueryModel()
                                                          {
                                                              FromCity = cityFrom1.Name,
                                                              FromCityTime = waypoint1.CityFromDepartTime,
                                                              ToCity = cityTo1.Name,
                                                              ToCityTime = waypoint1.CityToArrivalTime
                                                          }).ToList(),

                                             FromCityTime = (from waypoint1 in context.WayPointsSchedules
                                                             join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                                             join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                                             join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                                             where cityFrom1.Name == order.FromCityName
                                                             select waypoint1.CityFromDepartTime).First(),

                                             ToCityTime = (from waypoint1 in context.WayPointsSchedules
                                                           join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                                           join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                                           join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                                           where cityTo1.Name == order.ToCityName
                                                           select waypoint1.CityFromDepartTime).First(),
                                         }).ToList();




                //User.Identities.FirstOrDefault()?.Claims.Where(c => c.Type == "name").FirstOrDefault()?.Value
                ViewBag.UserOrders = fullUserOrderData;
            }

            return View();
        }

        public void MakeBooking(UserQueryModel user, SelectedRouteQueryModel route,List<Service> services, int seatNumber)
        {
            using (var context = new MapAppContext())
            {
                var waypointsSeats = (from transp in context.Transportations
                                 join bus in context.Buses on transp.BusId equals bus.Id
                                 join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                // join transpWaypoint in context.TransportationWaypoints on waypoint.Id equals transpWaypoint.WayPointsScheduleId
                                // join transpBusSeat in context.TransportationBusSeats on transpWaypoint.Id equals transpBusSeat.TransportationWaypointId
                                 join path in context.Paths on waypoint.PathId equals path.Id
                                 join cityFrom in context.Cities on path.CityFromId equals cityFrom.Id
                                 join cityTo in context.Cities on path.CityToId equals cityTo.Id
                                 where transp.Id == route.TransportationId
                                    && waypoint.Sequence >= (from waypoint2 in context.WayPointsSchedules
                                                           join path2 in context.Paths on waypoint2.PathId equals path2.Id
                                                           join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                                                           join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                                                           where cityFrom2.Name == route.FromCity
                                                            && waypoint2.BusId == bus.Id
                                                             select waypoint2.Sequence).First()
                                    && waypoint.Sequence <= (from waypoint2 in context.WayPointsSchedules
                                                            join path2 in context.Paths on waypoint2.PathId equals path2.Id
                                                            join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                                                            join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                                                            where cityTo2.Name == route.ToCity
                                                            && waypoint2.BusId == bus.Id
                                                            select waypoint2.Sequence).First()
                                   // && transpBusSeat.SeatNumber == seatNumber
                                 select new 
                                 {
                                     //TranspWaypointSeatId = transpBusSeat.Id,
                                     WaypointPrice = waypoint.Price,
                                     
                                 }).ToList();

                var servicesId = services.Select(s => s.Id).ToList();

                var servicesFromDb = context.Services.Where(s => servicesId.Contains(s.Id)).ToList();

                Order newBooking = new Order()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserFirstName = user.FName,
                    UserLastName = user.LName,
                    UserEmail = user.Email,
                    UserPhone = user.Phone,
                    UserBirthDate = new DateTime(int.Parse(user.Year), int.Parse(user.Month), int.Parse(user.Day)),
                    AdultsCount = 1,
                    ChildrenCount = 0,
                    FromCityName = route.FromCity,
                    ToCityName = route.ToCity,
                    TransportationId =route.TransportationId,
                    OrderStatusId = "1",
                    SeatNumber = seatNumber,
                    PaymentMethod = PaymentMethod.CashOnBoarding,
                    TotalPrice = waypointsSeats.Sum(w => w.WaypointPrice) + servicesFromDb.Sum(s => s.Price),                 
                };

                context.Orders.Add(newBooking);

                foreach (var service in servicesFromDb)
                {
                    context.ServicesInOrders.Add(new ServicesInOrder()
                    {
                        Id = Guid.NewGuid().ToString(),
                        OrderId = newBooking.Id,
                        ServiceId = service.Id,
                    });
                }

                //var transpWaypointsBusSeats = context.TransportationBusSeats.Where(t => waypointsSeats.Select(s => s.TranspWaypointSeatId).Contains(t.Id));
                //foreach (var seat in transpWaypointsBusSeats)
                //{
                //    seat.IsTaken = true;
                //}

                var test = 1;
                context.SaveChanges();


                   //(from transp in context.Transportations
                   //              join transpWaypoint in context.TransportationWaypoints on transp.Id equals transpWaypoint.TransportationId)
            }
             
        }

        public void MakeBookingWithPayment()
        {

        }
    }
}
