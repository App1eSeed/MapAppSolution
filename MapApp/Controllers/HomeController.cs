using MapApp.Models.EF;
using MapApp.Models.QueryModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.Common;
using System.Linq;

namespace MapApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

             //DbGenerator.Generate(busCountGen: 20);
            //DbGenerator.GenerateTransportations();
            return View();

        }
       
        public IActionResult RoutesOffers(string fromCity,string toCity, DateTime departDate)
        {
            using (var context = new MapAppContext())
            {

                TimeSpan time = departDate == DateTime.Today 
                    ? new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second)
                    : new TimeSpan(0,0,0);
                

                var routesOffers = (from bus in context.Buses
                                            join transportation in context.Transportations on bus.Id equals transportation.BusId
                                            join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                            join path in context.Paths on waypoint.PathId equals path.Id
                                            join cityFrom in context.Cities on path.CityFromId equals cityFrom.Id
                                            join cityTo in context.Cities on path.CityToId equals cityTo.Id
                                    where (from bus1 in context.Buses
                                           join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                                           join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                                           join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                           join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                           join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                           where cityFrom1.Name == fromCity
                                              && transportation1.Id == transportation.Id
                                           // && transportation.DepartTime >= time
                                           select transportation1.Id).Contains(transportation.Id)
                                       && (from bus1 in context.Buses
                                           join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                                           join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                                           join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                           join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                           join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                           where cityTo1.Name == toCity
                                              && transportation1.Id == transportation.Id
                                           //  && transportation.DepartTime >= time
                                           select transportation1.Id).Contains(transportation.Id)
                                    && waypoint.Sequence >= (from bus1 in context.Buses
                                                                join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                                                                join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                                                                join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                                                join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                                                join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                                                where cityFrom1.Name == fromCity
                                                                               && transportation1.Id == transportation.Id
                                                                select waypoint1.Sequence).First()
                                    && waypoint.Sequence <= (from bus1 in context.Buses
                                                             join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                                                             join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                                                             join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                                             join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                                             join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                                             where cityTo1.Name == toCity
                                                                           && transportation1.Id == transportation.Id
                                                             select waypoint1.Sequence).First()
                                   && transportation.DepartDate == new DateTime(departDate.Year, departDate.Month, departDate.Day)
                                            orderby bus.Id, waypoint.Sequence
                                            select new Route()
                                            {
                                                BusId = bus.Id,
                                                TransportationId = transportation.Id,
                                                CityFrom = cityFrom.Name,
                                                CityTo = cityTo.Name,
                                                BusOperator = bus.Operator,
                                                Sequence = waypoint.Sequence,
                                                DepartTime = waypoint.CityFromDepartTime,
                                                ArrivalTime = waypoint.CityToArrivalTime,
                                                DepartDate = transportation.DepartDate,
                                                ArrivalDate = transportation.ArrivalDate,
                                                Price = waypoint.Price
                                            }).ToList().GroupBy(g => g.TransportationId).Select(grp => grp.ToList()).ToList();

                ViewBag.RoutesOffers = routesOffers;

                //var routesBeforeTransfer = (from bus in context.Buses
                //                                       join transportation in context.Transportations on bus.Id equals transportation.BusId
                //                                       join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                //                                       join path in context.Paths on waypoint.PathId equals path.Id
                //                                       join cityFrom in context.Cities on path.CityFromId equals cityFrom.Id
                //                                       join cityTo in context.Cities on path.CityToId equals cityTo.Id
                //                                       //Получаем список городов в которых возможна пересадка при учете того что мы садимся в городе X а должны приехатиь в город Y
                //                                       from transferCity in 
                //                                            (from bus1 in context.Buses
                //                                             join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                             join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                             join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                             join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                             //Проверка наличия пункта отправки в полном маршруте автобуса
                //                                             where (from bus2 in context.Buses
                //                                                               join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                               join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                               join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                               join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                               where cityFrom2.Name == "Cherkasy"
                //                                                               select bus2.Id).ToList().Contains(bus1.Id)
                //                                            //Выборка только тех пунктов что больше по порядку чем необходимый пункт отправки
                //                                                && waypoint1.Sequence > (from bus2 in context.Buses
                //                                                               join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                               join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                               join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                               join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                               where cityFrom2.Name == "Cherkasy" && bus2.Id == bus1.Id
                //                                                               select waypoint2.Sequence).First()
                //                                              select cityTo1.Name)
                //                                   .Intersect(from bus1 in context.Buses
                //                                              join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                              join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                              join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                              join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                              //Проверка наличия пункта прибытия в полном маршруте автобуса
                //                                              where (from bus2 in context.Buses                                                                  
                //                                                               join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                               join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                               join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                               join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                               where cityTo2.Name == "Energodar"
                //                                                               select bus2.Id
                //                                                                          ).ToList().Contains(bus1.Id)
                //                                               //Выборка только тех пунктов что больше по порядку чем необходимый пункт прибытия
                //                                               && waypoint1.Sequence <= (from bus2 in context.Buses
                //                                                              join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                              join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                              join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                              join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                              where cityTo2.Name == "Energodar" && bus2.Id == bus1.Id
                //                                                              select waypoint2.Sequence
                //                                                               ).First()
                //                                                        select cityFrom1.Name
                //                                             )
                //                                        //Проверка что город отправления это заданый и в списек остановок присутствует город пересадки
                //                                        where (from bus1 in context.Buses
                //                                               join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                //                                               join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                               join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                               join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                               join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                               where cityFrom1.Name == "Cherkasy"
                //                                                  && (from bus2 in context.Buses
                //                                                      join transportation2 in context.Transportations on bus2.Id equals transportation2.BusId
                //                                                      join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                      join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                      join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                      join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                      where transportation2.Id == transportation1.Id
                //                                                      select cityTo2.Name).Contains(transferCity)
                //                                               select transportation1.Id).Contains(transportation.Id)
                //                                        //Проверка того что выборка начинаеться с заданого города
                //                                           && waypoint.Sequence >= (from bus1 in context.Buses
                //                                               join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                //                                               join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                               join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                               join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                               join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                               where cityFrom1.Name == "Cherkasy"
                //                                                  && transportation1.Id == transportation.Id
                //                                               select waypoint1.Sequence).First()
                //                                        //Проверка того что выборка заканчиваеться пунктом пересадки
                //                                           && waypoint.Sequence <= (from bus1 in context.Buses
                //                                               join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                //                                               join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                               join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                               join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                               join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                               where cityTo1.Name == transferCity
                //                                                 && transportation1.Id == transportation.Id
                //                                               select waypoint1.Sequence).First()
                //                                          && transportation.DepartDate == new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                //                                       orderby bus.Id, waypoint.Sequence
                //                                       select new 
                //                                       {
                //                                           BusId = bus.Id,
                //                                           CityFrom = cityFrom.Name,
                //                                           CityTo = cityTo.Name,
                //                                           Sequence = waypoint.Sequence,
                //                                           DepartTime = waypoint.CityFromDepartTime,
                //                                           ArrivalTime = waypoint.CityToArrivalTime,
                //                                           DepartDate = transportation.DepartDate,
                //                                           ArrivalDate = transportation.ArrivalDate
                //                                       }).ToList();

                //            var routesAfterTransfer = (from bus in context.Buses
                //                                         join transportation in context.Transportations on bus.Id equals transportation.BusId
                //                                         join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                //                                         join path in context.Paths on waypoint.PathId equals path.Id
                //                                         join cityFrom in context.Cities on path.CityFromId equals cityFrom.Id
                //                                         join cityTo in context.Cities on path.CityToId equals cityTo.Id
                //                                         //Получаем список городов в которых возможна пересадка при учете того что мы садимся в городе X а должны приехатиь в город Y
                //                                         from transferCity in
                //                                              (from bus1 in context.Buses
                //                                               join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                               join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                               join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                               join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                             //Проверка наличия пункта отправки в полном маршруте автобуса
                //                                             where (from bus2 in context.Buses
                //                                                      join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                      join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                      join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                      join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                      where cityFrom2.Name == "Cherkasy"
                //                                                      select bus2.Id).ToList().Contains(bus1.Id)
                //                                                  //Выборка только тех пунктов что больше по порядку чем необходимый пункт отправки
                //                                                  && waypoint1.Sequence > (from bus2 in context.Buses
                //                                                                           join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                                           join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                                           join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                                           join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                                           where cityFrom2.Name == "Cherkasy" && bus2.Id == bus1.Id
                //                                                                           select waypoint2.Sequence).First()
                //                                               select cityTo1.Name)
                //                                     .Intersect(from bus1 in context.Buses
                //                                                join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                                join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                                join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                                join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                              //Проверка наличия пункта прибытия в полном маршруте автобуса
                //                                              where (from bus2 in context.Buses
                //                                                       join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                       join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                       join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                       join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                       where cityTo2.Name == "Energodar"
                //                                                       select bus2.Id
                //                                                                            ).ToList().Contains(bus1.Id)
                //                                                 //Выборка только тех пунктов что больше по порядку чем необходимый пункт прибытия
                //                                                 && waypoint1.Sequence <= (from bus2 in context.Buses
                //                                                                           join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                                           join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                                           join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                                           join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                                           where cityTo2.Name == "Energodar" && bus2.Id == bus1.Id
                //                                                                           select waypoint2.Sequence
                //                                                                 ).First()
                //                                                select cityFrom1.Name
                //                                               )
                //                                             //Проверка что город отправления это заданый и в списек остановок присутствует город пересадки
                //                                         where (from bus1 in context.Buses
                //                                                join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                //                                                join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                                join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                                join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                                join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                                where cityTo1.Name == "Energodar"
                //                                                   && (from bus2 in context.Buses
                //                                                       join transportation2 in context.Transportations on bus2.Id equals transportation2.BusId
                //                                                       join waypoint2 in context.WayPointsSchedules on bus2.Id equals waypoint2.BusId
                //                                                       join path2 in context.Paths on waypoint2.PathId equals path2.Id
                //                                                       join cityFrom2 in context.Cities on path2.CityFromId equals cityFrom2.Id
                //                                                       join cityTo2 in context.Cities on path2.CityToId equals cityTo2.Id
                //                                                       where transportation2.Id == transportation1.Id
                //                                                       select cityTo2.Name).Contains(transferCity)
                //                                                select transportation1.Id).Contains(transportation.Id)
                //                                            //Проверка того что выборка начинаеться с заданого города
                //                                            && waypoint.Sequence <= (from bus1 in context.Buses
                //                                                                     join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                //                                                                     join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                                                     join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                                                     join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                                                     join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                                                     where cityTo1.Name == "Energodar"
                //                                                                       && transportation1.Id == transportation.Id
                //                                                                     select waypoint1.Sequence).First()
                //                                            //Проверка того что выборка заканчиваеться пунктом пересадки
                //                                            && waypoint.Sequence >= (from bus1 in context.Buses
                //                                                                     join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                //                                                                     join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                //                                                                     join path1 in context.Paths on waypoint1.PathId equals path1.Id
                //                                                                     join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                //                                                                     join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                //                                                                     where cityTo1.Name == transferCity
                //                                                                      && transportation1.Id == transportation.Id
                //                                                                     select waypoint1.Sequence).First()
                //                                            && transportation.DepartDate == new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                //                                         orderby bus.Id, waypoint.Sequence
                //                                         select new
                //                                         {
                //                                             BusId = bus.Id,
                //                                             CityFrom = cityFrom.Name,
                //                                             CityTo = cityTo.Name,
                //                                             Sequence = waypoint.Sequence,
                //                                             DepartTime = waypoint.CityFromDepartTime,
                //                                             ArrivalTime = waypoint.CityToArrivalTime,
                //                                             DepartDate = transportation.DepartDate,
                //                                             ArrivalDate = transportation.ArrivalDate
                //                                         }).ToList();


                //            //	ViewBag.RoutesOffers = routesOffers;
                //            ViewBag.Test = routesBeforeTransfer;
                //            ViewBag.Test = routesAfterTransfer;
            }

            return View();
        }

        [HttpPost]
        public IActionResult TripOptions(string fromCity, string toCity, string transportationId,DateTime returnalDate)
        {
            using (var context = new MapAppContext())
            {
                var freeSeats = (from transp in context.Transportations
                                 join transpWaypoint in context.TransportationWaypoints on transp.Id equals transpWaypoint.TransportationId
                                 join transpWaypointBusSeat in context.TransportationBusSeats on transpWaypoint.Id equals transpWaypointBusSeat.TransportationWaypointId
                                 join waypoint in context.WayPointsSchedules on transpWaypoint.WayPointsScheduleId equals waypoint.Id
                                 join path in context.Paths on waypoint.PathId equals path.Id
                                 join cityFrom in context.Cities on path.CityFromId equals cityFrom.Id
                                 join cityTo in context.Cities on path.CityToId equals cityTo.Id
                                 join groupedNumbers in (from transp2 in context.Transportations
                                       join transpWaypoint2 in context.TransportationWaypoints on transp2.Id equals transpWaypoint2.TransportationId
                                       join transpWaypointBusSeat2 in context.TransportationBusSeats on transpWaypoint2.Id equals transpWaypointBusSeat2.TransportationWaypointId
                                       join waypoint2 in context.WayPointsSchedules on transpWaypoint2.WayPointsScheduleId equals waypoint2.Id
                                       where transp2.Id == transportationId
                                          && waypoint2.Sequence >= (from bus3 in context.Buses
                                                                    join transp3 in context.Transportations on bus3.Id equals transp3.BusId
                                                                    join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
                                                                    join path3 in context.Paths on waypoint3.PathId equals path3.Id
                                                                    join cityFrom3 in context.Cities on path3.CityFromId equals cityFrom3.Id
                                                                    where cityFrom3.Name == fromCity
                                                                        && transp3.Id == transportationId
                                                                    select waypoint3.Sequence).First()
                                          && waypoint2.Sequence <= (from bus3 in context.Buses
                                                                    join transp3 in context.Transportations on bus3.Id equals transp3.BusId
                                                                    join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
                                                                    join path3 in context.Paths on waypoint3.PathId equals path3.Id
                                                                    join cityTo3 in context.Cities on path3.CityToId equals cityTo3.Id
                                                                    where cityTo3.Name == toCity
                                                                       && transp3.Id == transportationId
                                                                    select waypoint3.Sequence).First()
                                          && transpWaypointBusSeat2.IsTaken == false
                                       group transpWaypointBusSeat2 by transpWaypointBusSeat2.SeatNumber into SeatNumbersGroup
                                       select new { Number = SeatNumbersGroup.Key, Count = SeatNumbersGroup.Count() }) on transpWaypointBusSeat.SeatNumber equals groupedNumbers.Number
                                    where transp.Id == transportationId
                                    && transpWaypointBusSeat.IsTaken == false
                                    && cityFrom.Name == fromCity
                                    && groupedNumbers.Count > ((from bus3 in context.Buses
                                                                   join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
                                                                   join path3 in context.Paths on waypoint3.PathId equals path3.Id
                                                                   join cityTo3 in context.Cities on path3.CityToId equals cityTo3.Id
                                                                   where cityTo3.Name == toCity
                                                                   select waypoint3.Sequence).First()
                                                                      -
                                                                      (from bus3 in context.Buses
                                                                       join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
                                                                       join path3 in context.Paths on waypoint3.PathId equals path3.Id
                                                                       join cityFrom3 in context.Cities on path3.CityFromId equals cityFrom3.Id
                                                                       where cityFrom3.Name == fromCity
                                                                       select waypoint3.Sequence).First())
                                 orderby transpWaypointBusSeat.SeatNumber
                                 select transpWaypointBusSeat.SeatNumber).ToList();


                //var freeSeats = (from transp in context.Transportations
                //                 join transpWaypoint in context.TransportationWaypoints on transp.Id equals transpWaypoint.TransportationId
                //                 join transpWaypointBusSeat in context.TransportationBusSeats on transpWaypoint.Id equals transpWaypointBusSeat.TransportationWaypointId
                //                 join waypoint in context.WayPointsSchedules on transpWaypoint.WayPointsScheduleId equals waypoint.Id
                //                 join path in context.Paths on waypoint.PathId equals path.Id
                //                 join cityFrom in context.Cities on path.CityFromId equals cityFrom.Id
                //                 join cityTo in context.Cities on path.CityToId equals cityTo.Id
                //                 where transp.Id == transportationId
                //                    && transpWaypointBusSeat.IsTaken == false
                //                    && cityFrom.Name == fromCity
                //                    && (from transp1 in context.Transportations
                //                        join transpWaypoint1 in context.TransportationWaypoints on transp1.Id equals transpWaypoint1.TransportationId
                //                        join transpWaypointBusSeat1 in context.TransportationBusSeats on transpWaypoint1.Id equals transpWaypointBusSeat1.TransportationWaypointId
                //                        join groupedNumbers in (from transp2 in context.Transportations
                //                                                 join transpWaypoint2 in context.TransportationWaypoints on transp2.Id equals transpWaypoint2.TransportationId
                //                                                 join transpWaypointBusSeat2 in context.TransportationBusSeats on transpWaypoint2.Id equals transpWaypointBusSeat2.TransportationWaypointId
                //                                                 join waypoint2 in context.WayPointsSchedules on transpWaypoint2.WayPointsScheduleId equals waypoint2.Id
                //                                                 where transp2.Id == transportationId
                //                                                    && waypoint2.Sequence >= (from bus3 in context.Buses
                //                                                                             join transp3 in context.Transportations on bus3.Id equals transp3.BusId
                //                                                                             join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
                //                                                                             join path3 in context.Paths on waypoint3.PathId equals path3.Id
                //                                                                             join cityFrom3 in context.Cities on path3.CityFromId equals cityFrom3.Id
                //                                                                             where cityFrom3.Name == fromCity
                //                                                                                && transp3.Id == transportationId
                //                                                                             select waypoint3.Sequence).First()
                //                                                    && waypoint2.Sequence <= (from bus3 in context.Buses
                //                                                                              join transp3 in context.Transportations on bus3.Id equals transp3.BusId
                //                                                                              join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
                //                                                                              join path3 in context.Paths on waypoint3.PathId equals path3.Id
                //                                                                              join cityTo3 in context.Cities on path3.CityToId equals cityTo3.Id
                //                                                                              where cityTo3.Name == toCity
                //                                                                                 && transp3.Id == transportationId
                //                                                                              select waypoint3.Sequence).First()
                //                                                    && transpWaypointBusSeat2.IsTaken == false
                //                                                 group transpWaypointBusSeat2 by transpWaypointBusSeat2.SeatNumber into SeatNumbersGroup
                //                                                 select new { Number = SeatNumbersGroup.Key, Count = SeatNumbersGroup.Count() }) on transpWaypointBusSeat.SeatNumber equals groupedNumbers.Number
                //                        where transp1.Id == transportationId                                           
                //                           && transpWaypointBusSeat1.IsTaken == false
                //                           && groupedNumbers.Count > ((from bus3 in context.Buses
                //                                                      join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
                //                                                      join path3 in context.Paths on waypoint3.PathId equals path3.Id
                //                                                      join cityTo3 in context.Cities on path3.CityToId equals cityTo3.Id
                //                                                      where cityTo3.Name == toCity
                //                                                      select waypoint3.Sequence).First() 
                //                                                      -
                //                                                      (from bus3 in context.Buses
                //                                                      join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
                //                                                      join path3 in context.Paths on waypoint3.PathId equals path3.Id
                //                                                      join cityFrom3 in context.Cities on path3.CityFromId equals cityFrom3.Id
                //                                                      where cityFrom3.Name == fromCity
                //                                                      select waypoint3.Sequence).First())
                //                       select transpWaypointBusSeat1.SeatNumber).Contains(transpWaypointBusSeat.SeatNumber)                                         
                //                 orderby transpWaypointBusSeat.SeatNumber
                //                 select transpWaypointBusSeat.SeatNumber).ToList();

                var services = context.Services.ToList();



                ViewBag.FreeSeats = freeSeats;
                ViewBag.Services = services;


            }
            ViewBag.ReturnalDate = null;
            return View();




            //from transp in context.Transportations
            //join transpWaypoint in context.TransportationWaypoints on transp.Id equals transpWaypoint.TransportationId
            //join transpWaypointBusSeat in context.TransportationBusSeats on transpWaypoint.Id equals transpWaypointBusSeat.TransportationWaypointId
            //where (from transp2 in context.Transportations
            //       join transpWaypoint2 in context.TransportationWaypoints on transp2.Id equals transpWaypoint2.TransportationId
            //       join transpWaypointBusSeat2 in context.TransportationBusSeats on transpWaypoint2.Id equals transpWaypointBusSeat2.TransportationWaypointId
            //       join waypoint2 in context.WayPointsSchedules on transpWaypoint2.WayPointsScheduleId equals waypoint2.Id
            //       where transp2.Id == transportationId
            //          && waypoint2.Sequence >= (from bus3 in context.Buses
            //                                    join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
            //                                    join path3 in context.Paths on waypoint3.PathId equals path3.Id
            //                                    join cityFrom3 in context.Cities on path3.CityFromId equals cityFrom3.Id
            //                                    where cityFrom3.Name == fromCity
            //                                    select waypoint3.Sequence).First()
            //          && waypoint2.Sequence <= (from bus3 in context.Buses
            //                                    join waypoint3 in context.WayPointsSchedules on bus3.Id equals waypoint3.BusId
            //                                    join path3 in context.Paths on waypoint3.PathId equals path3.Id
            //                                    join cityTo3 in context.Cities on path3.CityToId equals cityTo3.Id
            //                                    where cityTo3.Name == toCity
            //                                    select waypoint3.Sequence).First()
            //          && transpWaypointBusSeat2.SeatNumber == transpWaypointBusSeat.SeatNumber
            //          && transpWaypointBusSeat2.IsTaken == false
            //       select waypoint2.Id).Contains(transpWaypoint.WayPointsScheduleId)
        }

        public IActionResult PassengerDetails()
        {
            ViewBag.ReturnalDate = null;
            return View();
        }

        public IActionResult ReviewAndPay()
        {
            ViewBag.ReturnalDate = null;
            return View();
        }

        public JsonResult GetAllCities()
        {
            using (var context = new MapAppContext())
            {
                var cities = (from city in context.Cities
                              select city.Name).ToList();

                return Json(cities);
            }
        }
    }
}
