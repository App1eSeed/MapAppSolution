using MapApp.Models.EF;
using MapApp.Models.QueryModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MapApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

             //DbGenerator.Generate(busCountGen: 10);
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
                                              && transportation.DepartTime >= time
                                           select transportation1.Id).Contains(transportation.Id)
                                       && (from bus1 in context.Buses
                                           join transportation1 in context.Transportations on bus1.Id equals transportation1.BusId
                                           join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                                           join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                           join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                           join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                           where cityTo1.Name == toCity
                                              && transportation.DepartTime >= time
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
                                                CityFrom = cityFrom.Name,
                                                CityTo = cityTo.Name,
                                                Sequence = waypoint.Sequence,
                                                DepartTime = waypoint.CityFromDepartTime,
                                                ArrivalTime = waypoint.CityToArrivalTime,
                                                DepartDate = transportation.DepartDate,
                                                ArrivalDate = transportation.ArrivalDate,
                                                Price = waypoint.Price
                                            }).ToList().GroupBy(g => g.BusId).Select(grp => grp.ToList()).ToList();

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

        public IActionResult TripOptions()
        {
            return View();
        }

        public IActionResult PassengerDetails()
        {
            return View();
        }

        public IActionResult ReviewAndPay()
        {
            return View();
        }
    }
}
