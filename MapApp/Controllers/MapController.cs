using MapApp.Models;
using MapApp.Models.ApiRequestModels;
using MapApp.Models.EF;
using MapApp.Models.EF.Entities;
using MapApp.Models.QueryModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MapApp.Controllers
{
    public class MapController : Controller
    {
        private readonly ILogger<MapController> _logger;

        private IMemoryCache cache;
        public MapController(ILogger<MapController> logger, IMemoryCache cache)
        {
            _logger = logger;
            this.cache = cache;
        }

        public IActionResult Map()
        {
            //DbGenerator.Generate(busCountGen: 10, wayPointsCount: 6);

            return View("../Home/Map");
        }
        public JsonResult GetFullRoute(string busId)
        {
            PathQuery fullRoute = new PathQuery();
            using (var context = new MapAppContext())
            {
                var fullRoutePathIds = (from bus in context.Buses
                                        join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                        join path in context.Paths on waypoint.PathId equals path.Id
                                        where bus.Id == busId
                                        orderby waypoint.Sequence
                                        select new
                                        {
                                            path.Id
                                        }).ToList();


                fullRoute.PathCoords = (from bus in context.Buses
                                        join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                        join path in context.Paths on waypoint.PathId equals path.Id
                                        join coords in context.Coords on path.Id equals coords.PathId
                                        where bus.Id == busId
                                        orderby waypoint.Sequence, coords.Id
                                        select new float[2]
                                        {
                                             coords.Longtitude,
                                             coords.Latitude
                                        }).ToList();
            }
            return Json(fullRoute);
        }

        //public JsonResult GetFullRoute(string busId)
        //{
           
        //    PathQuery fullRoute = new PathQuery();

        //    Dictionary<string, PathQuery> cacheResult = (Dictionary<string, PathQuery>)cache.Get("Paths") ?? new Dictionary<string, PathQuery>();

        //    using (var context = new MapAppContext())
        //    {
        //        var routes = (from bus in context.Buses
        //                      join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
        //                      where bus.Id == busId
        //                      orderby waypoint.Sequence
        //                      select new
        //                      {
        //                          waypoint.PathId
        //                      }).ToList();

        //        foreach (var route in routes)
        //        {
        //            if (cacheResult.ContainsKey(route.PathId))
        //            {

        //                fullRoute.PathCoords.AddRange(cacheResult[route.PathId].PathCoords);
        //            }
        //            else
        //            {

        //                PathQuery partialRoute = (from bus in context.Buses
        //                                          join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
        //                                          join path in context.Paths on waypoint.PathId equals path.Id
        //                                          where bus.Id == busId && waypoint.PathId == route.PathId
        //                                          select new PathQuery()
        //                                          {
        //                                              PathId = path.Id,
        //                                              Distance = path.Distance,
        //                                              Time = path.Time

        //                                          }).First();

        //                partialRoute.PathCoords = (from bus in context.Buses
        //                                           join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
        //                                           join path in context.Paths on waypoint.PathId equals path.Id
        //                                           join coords in context.Coords on path.Id equals coords.PathId
        //                                           where path.Id == route.PathId
        //                                           orderby coords.Id
        //                                           select new float[2]
        //                                           {
        //                                        coords.Longtitude,
        //                                        coords.Latitude
        //                                           }).ToList();

        //                fullRoute.PathCoords.AddRange(partialRoute.PathCoords);

        //                cacheResult.Add(route.PathId, partialRoute);

        //                cache.Set("Paths", cacheResult, new MemoryCacheEntryOptions
        //                {
        //                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(100)
        //                });
        //            }
        //        }

        //    }

        //    return Json(fullRoute);
        //}
        public JsonResult GetWayToNextCity(string busId, int sequence)
        {
            using (var context = new MapAppContext())
            {


                var busPath = (from bus in context.Buses
                               join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                               join path in context.Paths on waypoint.PathId equals path.Id
                               join coords in context.Coords on path.Id equals coords.PathId
                               where bus.Id == busId && waypoint.Sequence == sequence
                               select new float[2]
                               {
                                   coords.Longtitude,
                                   coords.Latitude
                               }).ToArray();

                return Json(busPath);
            }
        }
        public JsonResult GetPath(string busId, TimeSpan? busDepartTime, int sequence = 1 )
        {
            PathQuery busPath;
            dynamic nextWaypoint;
            Dictionary<string, PathQuery> cacheResult = (Dictionary<string, PathQuery>)cache.Get("Paths") ?? new Dictionary<string, PathQuery>();

            using (var context = new MapAppContext())
            {
                var pathTest = (from bus in context.Buses
                              join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                              join path in context.Paths on waypoint.PathId equals path.Id
                              where bus.Id == busId && waypoint.Sequence == sequence
                              select new 
                              {
                                  path.Id
                              }).ToList().FirstOrDefault();
                if (pathTest == null)
                {
                    return Json(new PathQuery());
                }
                if (pathTest != null && cacheResult.ContainsKey(pathTest.Id))
                {

                    busPath = cacheResult[pathTest.Id];
                }
                else
                {
                
                    busPath = (from bus in context.Buses
                                    join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                    join path in context.Paths on waypoint.PathId equals path.Id
                                    where bus.Id == busId && waypoint.Sequence == sequence
                                    select new PathQuery()
                                    {
                                        PathId = path.Id,
                                        Distance = path.Distance,
                                        Time = path.Time
                                        
                                    }).FirstOrDefault();


                    busPath.PathCoords = (from bus in context.Buses
                                          join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                          join path in context.Paths on waypoint.PathId equals path.Id
                                          join coords in context.Coords on path.Id equals coords.PathId
                                          where waypoint.BusId == busId && waypoint.Sequence == sequence
                                          orderby coords.Id
                                          select new float[2]
                                          {
                                            coords.Longtitude,
                                            coords.Latitude
                                          }).ToList();


                    if (!cacheResult.ContainsKey(busPath.PathId))
                    {
                        cacheResult.Add(busPath.PathId, busPath);
                    }
                    cache.Set("Paths", cacheResult, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(100)
                    });
                }

                nextWaypoint = (from bus in context.Buses
                                   join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                   where bus.Id == busId && waypoint.Sequence == sequence + 1
                                   select new
                                   {
                                       CityFromDepartTime = waypoint.CityFromDepartTime,
                                       CityToArrivalTime = waypoint.CityToArrivalTime
                                   }).ToList().FirstOrDefault();
               
            }
            if (busDepartTime != null)
            {
                int difTime;
                //if (nextWaypoint == null)
                //{
                    difTime = (int)(busDepartTime.Value.TotalSeconds > DateTime.Now.TimeOfDay.TotalSeconds 
                        ? DateTime.Now.TimeOfDay.TotalSeconds + 86400 - busDepartTime.Value.TotalSeconds
                        : DateTime.Now.TimeOfDay.TotalSeconds - busDepartTime.Value.TotalSeconds);
                //}
                //else
                //{
                //    difTime = (int)(nextWaypoint.CityToArrivalTime.TotalSeconds < busDepartTime.Value.TotalSeconds
                //   ? DateTime.Now.TimeOfDay.TotalSeconds + 86400 - busDepartTime.Value.TotalSeconds
                //   : DateTime.Now.TimeOfDay.TotalSeconds - busDepartTime.Value.TotalSeconds);
                //}
               
                float speed = (busPath.Distance * 1000) / busPath.Time;

                float difDistance = difTime * speed;
                float metresForPoint = (busPath.Distance * 1000 / busPath.PathCoords.Count);

                int count = busPath.PathCoords.Count;

                var points = busPath.PathCoords.Skip((int)(difDistance / metresForPoint)).ToList();
                PathQuery shortenedBusPuth;
                if (nextWaypoint == null)
                {
                    shortenedBusPuth = new PathQuery()
                    {
                        PathId = busPath.PathId,
                        Distance = busPath.Distance,
                        Time = busPath.Time,
                        Speed = speed - 3,
                        PathCoords = points,
                    };
                }
                else
                {
                    shortenedBusPuth = new PathQuery()
                    {
                        PathId = busPath.PathId,
                        Distance = busPath.Distance,
                        Time = busPath.Time,
                        Speed = speed - 3,
                        PathCoords = points,
                        NextDepartTime = nextWaypoint.CityFromDepartTime.ToString(@"hh\:mm\:ss")

                    };
                }

                return Json(shortenedBusPuth);
            }
            else
            {
                busPath.Speed = (busPath.Distance * 1000) / busPath.Time;
                return Json(busPath);
            }
            
     
        }
        public JsonResult GetWaypointsForRoute(string busId)
        {
            using (var context = new MapAppContext())
            {
                var test = (from innerBus in context.Buses
                            join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                            where innerBus.Id == busId
                            orderby innerWaypoint.Sequence
                            select new
                            {
                                innerWaypoint.Sequence
                            }).Last().Sequence;
                var waypoints = (from bus in context.Buses
                                 join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                 join path in context.Paths on waypoint.PathId equals path.Id
                                 where bus.Id == busId
                                 orderby waypoint.Sequence
                                 select new WaypointsInfoForBus()
                                 {
                                     City = path.CityFromId
                                 }).ToList()
                                 .Union((from bus in context.Buses
                                        join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                        join path in context.Paths on waypoint.PathId equals path.Id
                                        where bus.Id == busId && waypoint.Sequence == (from innerBus in context.Buses
                                                                                       join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                                                       where innerBus.Id == busId
                                                                                       orderby innerWaypoint.Sequence
                                                                                       select new
                                                                                       {
                                                                                           innerWaypoint.Sequence
                                                                                       }).Last().Sequence
                                        select new WaypointsInfoForBus()
                                        {
                                            City = path.CityToId
                                        }).ToList());
                return Json(waypoints);
            }
        }
        [HttpPost]
        public JsonResult GetVisibleRoutes(TimeSpan time, HashSet<string> existingRoutes,float topLat=54, float topLong=52, float botLat=43, float botLong=18 )
        {

            int correction = 0;
            using (var context = new MapAppContext())
            {


                var currentDay = new DayOfWeekCheck() { Day = DateTime.Now.DayOfWeek };
                int currentTimeInSeconds = (int)time.TotalSeconds;
                //Add TimeInSeconds column for waypoints
                //fix days and time

                var query = (from bus in context.Buses
                              join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                              join path in context.Paths on waypoint.PathId equals path.Id
                              join city in context.Cities on path.CityFromId equals city.Id
                              join days in ((from innerWaypoint in context.WayPointsSchedules
                                             join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointsScheduleId
                                             select new DayOfWeekCheck()
                                             {
                                                 Id = innerWaypoint.Id,
                                                 Day = innerSchedule.DepartDay
                                             }).Union(from innerBus in context.Buses
                                                      join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                      join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointsScheduleId
                                                      where innerWaypoint.Sequence == (from innerBus in context.Buses
                                                                                       join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                                                       orderby innerWaypoint.Sequence
                                                                                       select new
                                                                                       {
                                                                                           innerWaypoint.Sequence
                                                                                       })
                                                                                                                          .Last().Sequence
                                                      select new DayOfWeekCheck()
                                                      {
                                                          Id = innerWaypoint.Id,
                                                          Day = innerSchedule.ArrivalDay
                                                      })).Distinct() on waypoint.Id equals days.Id
                              where !existingRoutes.Contains(bus.Id)
                                 && days.Day == currentDay.Day
                                 && (waypoint.CityFromDepartTimeInSec > waypoint.CityToArrivalTimeInSec
                                      ? (time.TotalSeconds + 86400 >= waypoint.CityFromDepartTimeInSec) && (time.TotalSeconds + 86400 <= waypoint.CityToArrivalTimeInSec + 86400)
                                      : (time.TotalSeconds >= waypoint.CityFromDepartTimeInSec) && (time.TotalSeconds <= waypoint.CityToArrivalTimeInSec))
                                 && (city.Latitude >= botLat - correction && city.Longtitude >= botLong - correction)
                                 && (city.Latitude <= topLat + correction && city.Longtitude <= topLong + correction)
                              select new
                              {
                                  BusId = bus.Id,
                                  Sequence = waypoint.Sequence,
                                  DepartTime = waypoint.CityFromDepartTime.ToString(),
                                  Country = (from innerBus in context.Buses
                                             join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                             join innerPath in context.Paths on innerWaypoint.PathId equals innerPath.Id
                                             join innerCity in context.Cities on innerPath.CityFromId equals innerCity.Id
                                             join innerCountry in context.Countries on innerCity.CountryId equals innerCountry.Id
                                             where innerWaypoint.Sequence == 1 && innerWaypoint.BusId == bus.Id
                                             select new
                                             {
                                                 innerCountry.Name
                                             }).First().Name
                              });

                var routes = (from bus in context.Buses
                              join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                              join path in context.Paths on waypoint.PathId equals path.Id
                              join city in context.Cities on path.CityFromId equals city.Id
                            join days in ((from innerWaypoint in context.WayPointsSchedules
                                           join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointsScheduleId
                                           select new DayOfWeekCheck()
                                           {
                                               Id = innerWaypoint.Id,
                                               Day = innerSchedule.DepartDay
                                           }).Union(from innerBus in context.Buses
                                                    join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                    join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointsScheduleId
                                                    where innerWaypoint.Sequence == (from innerBus in context.Buses
                                                                                                              join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId                                                                                                             
                                                                                                              orderby innerWaypoint.Sequence
                                                                                                              select new
                                                                                                              {
                                                                                                                  innerWaypoint.Sequence
                                                                                                              })
                                                                                                                        .Last().Sequence
                                                    select new DayOfWeekCheck()
                                                    {
                                                        Id = innerWaypoint.Id,
                                                        Day = innerSchedule.ArrivalDay
                                                    })).Distinct() on waypoint.Id equals days.Id
                              where !existingRoutes.Contains(bus.Id)
                                 &&  days.Day == currentDay.Day
                                 &&  (waypoint.CityFromDepartTimeInSec > waypoint.CityToArrivalTimeInSec
                                      ? (time.TotalSeconds + 86400 >= waypoint.CityFromDepartTimeInSec) && (time.TotalSeconds + 86400 <= waypoint.CityToArrivalTimeInSec + 86400)
                                      : (time.TotalSeconds >= waypoint.CityFromDepartTimeInSec) && (time.TotalSeconds <= waypoint.CityToArrivalTimeInSec))
                                 && (city.Latitude >= botLat - correction && city.Longtitude >= botLong - correction)
                                 && (city.Latitude <= topLat + correction && city.Longtitude <= topLong + correction)
                              select new
                              {
                                  BusId = bus.Id,
                                  Sequence = waypoint.Sequence,
                                  DepartTime = waypoint.CityFromDepartTime.ToString(),
                                  Country = (from innerBus in context.Buses
                                             join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                             join innerPath in context.Paths on innerWaypoint.PathId equals innerPath.Id
                                             join innerCity in context.Cities on innerPath.CityFromId equals innerCity.Id
                                             join innerCountry in context.Countries on innerCity.CountryId equals innerCountry.Id
                                             where innerWaypoint.Sequence == 1 && innerWaypoint.BusId == bus.Id
                                             select new
                                             {
                                                 innerCountry.Name
                                             }).First().Name
                              }).ToList();

                return Json(routes);
            }

        }
        public JsonResult GetNewRoutesStep(float centerLat, float centerLng, int zoom, TimeSpan time)
        {
            var currentDay = new DayOfWeekCheck() { Day = DateTime.Now.DayOfWeek };
            int currentTimeInSeconds = (int)time.TotalSeconds;
            int correction = 10;

            Dictionary<string, PathQuery> cacheResult = (Dictionary<string, PathQuery>)cache.Get("Paths") ?? new Dictionary<string, PathQuery>();

            using (var context = new MapAppContext())
            {
                var routes = (from bus in context.Buses
                              join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                              join path in context.Paths on waypoint.PathId equals path.Id
                              join city in context.Cities on path.CityFromId equals city.Id
                              join days in ((from innerWaypoint in context.WayPointsSchedules
                                             join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointsScheduleId
                                             select new DayOfWeekCheck()
                                             {
                                                 Id = innerWaypoint.Id,
                                                 Day = innerSchedule.DepartDay
                                             }).Union(from innerBus in context.Buses
                                                      join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                      join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointsScheduleId
                                                      where innerWaypoint.Sequence == (from innerBus in context.Buses
                                                                                       join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                                                       orderby innerWaypoint.Sequence
                                                                                       select new
                                                                                       {
                                                                                           innerWaypoint.Sequence
                                                                                       })
                                                                                                                          .Last().Sequence
                                                      select new DayOfWeekCheck()
                                                      {
                                                          Id = innerWaypoint.Id,
                                                          Day = innerSchedule.ArrivalDay
                                                      })).Distinct() on waypoint.Id equals days.Id
                              where days.Day == currentDay.Day
                                 && (waypoint.CityFromDepartTimeInSec > waypoint.CityToArrivalTimeInSec
                                      ? (currentTimeInSeconds + 86400 >= waypoint.CityFromDepartTimeInSec) && (currentTimeInSeconds + 86400 <= waypoint.CityToArrivalTimeInSec + 86400)
                                      : (currentTimeInSeconds >= waypoint.CityFromDepartTimeInSec) && (currentTimeInSeconds <= waypoint.CityToArrivalTimeInSec))
                                 && (city.Latitude >= centerLat - correction && city.Longtitude >= centerLng - correction)
                                 && (city.Latitude <= centerLat + correction && city.Longtitude <= centerLng + correction)
                              select new
                              {
                                  BusId = bus.Id,
                                  Sequence = waypoint.Sequence,
                                  DepartTime = waypoint.CityFromDepartTime.ToString(),

                                  CurrentLatLng = (from innerBus in context.Buses
                                                   join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                   join innerPath in context.Paths on innerWaypoint.PathId equals innerPath.Id
                                                   join innerCoords in context.Coords on innerPath.Id equals innerCoords.PathId
                                                   //where innerBus.Id == bus.Id && innerWaypoint.Sequence == waypoint.Sequence
                                                   //orderby innerCoords.Id
                                                   select new float[2]
                                                  {
                                                    innerCoords.Longtitude,
                                                    innerCoords.Latitude
                                                  }).Skip(Convert.ToInt32((waypoint.CityFromDepartTimeInSec > currentTimeInSeconds
                                                        ? currentTimeInSeconds + 86400 - waypoint.CityFromDepartTimeInSec
                                                        : currentTimeInSeconds - waypoint.CityFromDepartTimeInSec) *
                                                        (path.Distance * 1000 / path.Time) / ((path.Distance * 1000) / path.CoordsCount))).First(),

                                  Country = (from innerBus in context.Buses
                                             join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                             join innerPath in context.Paths on innerWaypoint.PathId equals innerPath.Id
                                             join innerCity in context.Cities on innerPath.CityFromId equals innerCity.Id
                                             join innerCountry in context.Countries on innerCity.CountryId equals innerCountry.Id
                                             where innerWaypoint.Sequence == 1 && innerWaypoint.BusId == bus.Id
                                             select new
                                             {
                                                 innerCountry.Name
                                             }).First().Name
                              });//.ToList();


                return Json(routes);
            }            
        }
        public JsonResult GetAllCities()
        {
            using (var context = new MapAppContext())
            {
                var cities = context.Cities.ToList();
                List<GeoJsonModel> geoCities = new List<GeoJsonModel>();
                foreach (var city in cities)
                {
                    geoCities.Add(new GeoJsonModel()
                    {
                        Geometry = new Geometry("Point", new List<float>() { city.Longtitude, city.Latitude }),
                        Type = "Feature",
                        Properties = new Properties()
                    });
                }
                return Json(geoCities);
            }

        }
        [HttpPost]
        public JsonResult GetBusInfo(string busId)
        {
            using (var context = new MapAppContext())
            {
                dynamic route;
                try
                {
                     route = (from waypoint in context.WayPointsSchedules
                                 join path in context.Paths on waypoint.PathId equals path.Id
                                 join city in context.Cities on path.CityFromId equals city.Id
                                 join coord in context.Coords on path.Id equals coord.PathId
                                 where waypoint.BusId == busId
                                 select new
                                 {
                                     Sequence = waypoint.Sequence,
                                     city = city.Name,
                                     coord.Longtitude,
                                     coord.Latitude
                                 }).ToList().OrderBy(r => r.Sequence);
                }
                catch (Exception)
                {
                    route = new { };
                }

                return Json(route);

            }
        }
      
        public IActionResult FillInfoPanel(string busId)
        {
            var busInfo = new List<WaypointsInfoForBus>();
            dynamic busSchedule;
            using (var context = new MapAppContext())
            {
                busInfo = ((from bus in context.Buses
                                         join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                         join path in context.Paths on waypoint.PathId equals path.Id
                                         join city in context.Cities on path.CityFromId equals city.Id
                                         where bus.Id == busId
                                         orderby waypoint.Sequence
                                         select new WaypointsInfoForBus()
                                         {
                                             City = city.Name,
                                             CityToArrivalTime = waypoint.CityToArrivalTime.ToString(@"hh\:mm"),
                                             CityFromDepartTime = waypoint.CityFromDepartTime.ToString(@"hh\:mm")
                                         })
                                         .ToList()
                                         .Union((from bus in context.Buses
                                                            join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                                            join path in context.Paths on waypoint.PathId equals path.Id
                                                            join city in context.Cities on path.CityToId equals city.Id
                                                            where bus.Id == busId && waypoint.Sequence == (from innerBus in context.Buses
                                                                                                        join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                                                                        where innerBus.Id == busId
                                                                                                        orderby innerWaypoint.Sequence
                                                                                                        select new
                                                                                                        {
                                                                                                            innerWaypoint.Sequence
                                                                                                        })
                                                                                                        .Last().Sequence
                                                            select new WaypointsInfoForBus()
                                                            {
                                                                City = city.Name,
                                                                CityToArrivalTime = waypoint.CityToArrivalTime.ToString(@"hh\:mm"),
                                                                CityFromDepartTime = waypoint.CityFromDepartTime.ToString(@"hh\:mm")
                                                            })
                                                            .ToList())).ToList();

                 busSchedule = (from bus in context.Buses
                                       where bus.Id == busId
                                       select new BusInfoPanelSchedule()
                                       {
                                           Id = bus.Id,
                                           Operator = bus.Operator,
                                          // Schedule = String.Join(", ", bus.Schedule.OrderBy(d => d.Day).Select(d => d.Day.ToString()).ToArray())
                                       }).ToList().First();

                
            }

            ViewBag.WaypointInfo = busInfo;
            ViewBag.BusSchedule = busSchedule;


            return PartialView("~/Views/PartialViews/InfoPanelInsides.cshtml");

        }

        public JsonResult GetRouteFromCityToCity(string busId, string fromCity, string toCity)
        {
            string category = Request.QueryString.Value;
            PathQuery fullRoute = new PathQuery();

            Dictionary<string, PathQuery> cacheResult = (Dictionary<string, PathQuery>)cache.Get("Paths") ?? new Dictionary<string, PathQuery>();


            using (var context = new MapAppContext())
            {

                var routes = (from bus in context.Buses
                              join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                              //join path in context.Paths on waypoint.PathId equals path.Id
                              //join cityFrom in context.Cities on waypoint
                              where bus.Id == busId
                                 && waypoint.Sequence >= (from bus1 in context.Buses
                                        join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                                        join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                        join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                        join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                        where cityFrom1.Name == fromCity
                                           && bus1.Id == busId
                                        select waypoint1.Sequence).First()
                                 && waypoint.Sequence <= (from bus1 in context.Buses
                                        join waypoint1 in context.WayPointsSchedules on bus1.Id equals waypoint1.BusId
                                        join path1 in context.Paths on waypoint1.PathId equals path1.Id
                                        join cityFrom1 in context.Cities on path1.CityFromId equals cityFrom1.Id
                                        join cityTo1 in context.Cities on path1.CityToId equals cityTo1.Id
                                        where cityTo1.Name == toCity
                                           && bus1.Id == busId
                                        select waypoint1.Sequence).First()
                              orderby waypoint.Sequence
                              select new
                              {
                                  waypoint.PathId
                              }).ToList();

                foreach (var route in routes)
                {
                    

                    if (cacheResult.ContainsKey(route.PathId))
                    {

                        fullRoute.PathCoords.AddRange(cacheResult[route.PathId].PathCoords);
                    }
                    else
                    {

                        PathQuery partialRoute = (from bus in context.Buses
                                                  join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                                  join path in context.Paths on waypoint.PathId equals path.Id
                                                  where bus.Id == busId && waypoint.PathId == route.PathId
                                                  select new PathQuery()
                                                  {
                                                      PathId = path.Id,
                                                      Distance = path.Distance,
                                                      Time = path.Time

                                                  }).First();

                        partialRoute.PathCoords = (from bus in context.Buses
                                                   join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                                   join path in context.Paths on waypoint.PathId equals path.Id
                                                   join coords in context.Coords on path.Id equals coords.PathId
                                                   where path.Id == route.PathId
                                                   orderby path.Id,coords.Id
                                                   select new float[2]
                                                   {
                                                coords.Longtitude,
                                                coords.Latitude
                                                   }).ToList();

                        fullRoute.PathCoords.AddRange(partialRoute.PathCoords);

                        cacheResult.Add(route.PathId, partialRoute);

                        cache.Set("Paths", cacheResult, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(100)
                        });
                    }
                    
                }

                
            }

            return Json(fullRoute);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
