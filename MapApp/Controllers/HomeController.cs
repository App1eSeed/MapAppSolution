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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static readonly HttpClient client = new HttpClient();
        private IMemoryCache cache;
        public HomeController(ILogger<HomeController> logger, IMemoryCache cache)
        {
            _logger = logger;
            this.cache = cache;
        }

        public IActionResult Index()
        {
            //DbGenerator.Generate(busCountGen: 10, wayPointsCount: 6);

            return View();
        }
        //public JsonResult GetFullRoute(string busId)
        //{
        //    PathQuery fullRoute = new PathQuery();
        //    using (var context = new MapAppContext())
        //    {
        //        var fullRoutePathIds = (from bus in context.Buses
        //                                join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
        //                                join path in context.Paths on waypoint.PathId equals path.Id
        //                                where bus.Id == busId
        //                                orderby waypoint.Sequence
        //                                select new
        //                                {
        //                                    path.Id
        //                                }).ToList();


        //        fullRoute.PathCoords = (from bus in context.Buses
        //                                join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
        //                                join path in context.Paths on waypoint.PathId equals path.Id
        //                                join coords in context.Coords on path.Id equals coords.PathId
        //                                where bus.Id == busId
        //                                orderby waypoint.Sequence, coords.Id
        //                                select new float[2]
        //                                {
        //                                     coords.Longtitude,
        //                                     coords.Latitude
        //                                }).ToList();
        //    }
        //    return Json(fullRoute);
        //}

        public JsonResult GetFullRoute(string busId)
        {
            PathQuery fullRoute = new PathQuery();

            Dictionary<string, PathQuery> cacheResult = (Dictionary<string, PathQuery>)cache.Get("Paths") ?? new Dictionary<string, PathQuery>();

            using (var context = new MapAppContext())
            {
                var routes = (from bus in context.Buses
                              join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                              where bus.Id == busId
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
                                                   orderby coords.Id
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
        public JsonResult GetVisibleRoutes(float topLat, float topLong, float botLat, float botLong, TimeSpan time, HashSet<string> existingRoutes)
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
                                             join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointId
                                             select new DayOfWeekCheck()
                                             {
                                                 Id = innerWaypoint.Id,
                                                 Day = innerSchedule.DepartDay
                                             }).Union(from innerBus in context.Buses
                                                      join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                      join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointId
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
                                           join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointId
                                           select new DayOfWeekCheck()
                                           {
                                               Id = innerWaypoint.Id,
                                               Day = innerSchedule.DepartDay
                                           }).Union(from innerBus in context.Buses
                                                    join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                    join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointId
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
                                             join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointId
                                             select new DayOfWeekCheck()
                                             {
                                                 Id = innerWaypoint.Id,
                                                 Day = innerSchedule.DepartDay
                                             }).Union(from innerBus in context.Buses
                                                      join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                                      join innerSchedule in context.Schedules on innerWaypoint.Id equals innerSchedule.WayPointId
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


        public static class DbGenerator
        {
            private static List<City> capitalsCities;
            private static List<City> adminCities;
            private static List<City> minorCities;
            private static List<City> commonCities;
            private static int[] departMinutes;
            private static List<Path> allPaths;

            private static int lastInsertedCoordsId { get; set; }
            static DbGenerator()
            {
                departMinutes = new int[4] { 0, 15, 30, 45 };

                using var context = new MapAppContext();
                    
                capitalsCities = context.Cities.Where(c => c.Capital == Capital.primary).ToList();
                adminCities = context.Cities.Where(c => c.Capital == Capital.admin).ToList();
                minorCities = context.Cities.Where(c => c.Capital == Capital.minor).ToList();
                commonCities = context.Cities.Where(c => c.Capital == Capital.none).ToList();               
                
            }

            public static void Generate(float searchRange = 0.5f, int stopTime = 15, int busCountGen = 1, int wayPointsCount = 6)
            {


                using (var context = new MapAppContext())
                {
                    for (int i = 0; i < busCountGen; i++)
                    {
                        allPaths = context.Paths.ToList();

                        if (context.Coords.OrderBy(p => p.Id).LastOrDefault() != null)
                        {
                            lastInsertedCoordsId = context.Coords.OrderBy(p => p.Id).LastOrDefault().Id;
                        }
                        else
                        {
                            lastInsertedCoordsId = 1;
                        }

                        var DataGen = CreateData(searchRange, stopTime, wayPointsCount, lastInsertedCoordsId);




                        context.Buses.Add(DataGen.Bus);
                        context.Paths.AddRange(DataGen.Paths);

                        
                        context.WayPointsSchedules.AddRange(DataGen.WayPointsSchedules);

                        //int k = 0;
                        //for (int j = 0; j < DataGen.WayPointsSchedules.Count; j++)
                        //{
                        //    if (DataGen.WayPointsSchedules[j].PathId == null)
                        //    {
                        //        DataGen.WayPointsSchedules[j].PathId = DataGen.Paths[k].Id;
                        //        k++;
                        //    }
                        //    DataGen.WayPointsSchedules[j].BusId = DataGen.Bus.Id;
                        //}

                        context.Coords.AddRange(DataGen.Coords);
                        context.Schedules.AddRange(DataGen.Schedules);
                        context.Database.OpenConnection();
                        try
                        {
                            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Coords ON;");
                            context.SaveChanges();
                            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Coords OFF;");
                        }
                        finally
                        {
                            context.Database.CloseConnection();
                        }
                    }
                }
                
            }

            private static DataGen CreateData(float searchRange, int stopTime, int pathCount, int LocalLastInsertedCoordsId)
            {
                var dataGen = new DataGen();
                Random r = new Random();

                Bus instBus = new Bus()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Operator = "Rand " + r.Next(1,10),
                };

                //Generated Bus
                dataGen.Bus = instBus;


                City startCity = GetDepCityByType(GetDepArrCityType());
                City endCity = GetArrCityByType(GetDepArrCityType(), startCity.Latitude, startCity.Longtitude, pathCount, searchRange * 2);               

                float latDiff = endCity.Latitude - startCity.Latitude,
                      lngDiff = endCity.Longtitude - startCity.Longtitude;

                List<City> excludedCities = new List<City>();
                excludedCities.Add(startCity);
                excludedCities.Add(endCity);

                City city1 = startCity;
                City city2 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, 1, pathCount,excludedCities, searchRange);

                List<Path> paths = new List<Path>();
                List<WayPointsSchedule> wayPoints = new List<WayPointsSchedule>();
                List<DayOfWeek> differentDays = new List<DayOfWeek>();
                DayOfWeek departDay = (DayOfWeek)r.Next(0, 7);
                DayOfWeek arrivalDay = departDay;
                differentDays.Add(departDay);            

                for (int j = 0; j < pathCount; j++)
                {
                    Path path = new Path();


                    if (j == pathCount - 1)
                    {
                        if (j % 2 == 0)
                        {
                            city2 = endCity;

                            var check = allPaths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList();
                            check.AddRange(paths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList());


                            if (check.Count > 0)
                            {
                                paths.Add(check[0]);
                                path = check[0];
                            }
                            else
                            {
                                path = new Path()
                                {
                                    Id = Guid.NewGuid().ToString("N"),
                                    CityFromId = city1.Id,
                                    CityToId = city2.Id
                                };

                                var CoordsBetweenCities = GetWayBetweenCities
                                    (ref path,
                                    city1,
                                    city2,
                                    LocalLastInsertedCoordsId
                                    );
                                if (CoordsBetweenCities != null)
                                {
                                    dataGen.Coords.AddRange(CoordsBetweenCities);
                                }
                                else
                                {
                                    return CreateData(searchRange, stopTime, pathCount, LocalLastInsertedCoordsId - dataGen.Coords.Count);
                                }
                                dataGen.Paths.Add(path);
                                paths.Add(path);
                                LocalLastInsertedCoordsId = lastInsertedCoordsId + dataGen.Coords.Count;

                            }
                        }
                        else
                        {
                            city1 = endCity;

                            var check = allPaths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList();
                            check.AddRange(paths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList());

                            if (check.Count > 0)
                            {
                                paths.Add(check[0]);
                                path = check[0];
                            }
                            else
                            {
                                path = new Path()
                                {
                                    Id = Guid.NewGuid().ToString("N"),
                                    CityFromId = city2.Id,
                                    CityToId = city1.Id
                                };


                                var CoordsBetweenCities = GetWayBetweenCities
                                    (ref path,
                                    city2,
                                    city1,
                                    LocalLastInsertedCoordsId
                                    );
                                if (CoordsBetweenCities != null)
                                {
                                    dataGen.Coords.AddRange(CoordsBetweenCities);
                                }
                                else
                                {
                                    return CreateData(searchRange, stopTime, pathCount, LocalLastInsertedCoordsId - dataGen.Coords.Count);
                                }
                                dataGen.Paths.Add(path);
                                paths.Add(path);
                                LocalLastInsertedCoordsId = lastInsertedCoordsId + dataGen.Coords.Count;

                            }
                        }
                    }
                    else if (j % 2 == 0)
                    {

                        city2 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, j + 1, pathCount, excludedCities, searchRange);

                        excludedCities.Add(city2);

                        var check = allPaths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList();
                        check.AddRange(paths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList());


                        if (check.Count > 0)
                        {
                            paths.Add(check[0]);
                            path = check[0];
                        }
                        else
                        {
                            path = new Path()
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                CityFromId = city1.Id,
                                CityToId = city2.Id
                            };

                            var CoordsBetweenCities = GetWayBetweenCities
                                     (ref path,
                                     city1,
                                     city2,
                                     LocalLastInsertedCoordsId
                                     );
                            if (CoordsBetweenCities != null)
                            {
                                dataGen.Coords.AddRange(CoordsBetweenCities);
                            }
                            else
                            {
                                return CreateData(searchRange, stopTime, pathCount, LocalLastInsertedCoordsId - dataGen.Coords.Count);
                            }
                            dataGen.Paths.Add(path);
                            paths.Add(path);
                            LocalLastInsertedCoordsId = lastInsertedCoordsId + dataGen.Coords.Count;

                        }
                    }
                    else
                    {

                        city1 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, j + 1, pathCount,excludedCities, searchRange);


                        excludedCities.Add(city1);

                        var check = allPaths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList();
                        check.AddRange(paths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList());

                        if (check.Count > 0)
                        {
                            paths.Add(check[0]);
                            path = check[0];
                        }
                        else
                        {
                            path = new Path()
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                CityFromId = city2.Id,
                                CityToId = city1.Id
                            };

                            var CoordsBetweenCities = GetWayBetweenCities
                                     (ref path,
                                     city1,
                                     city2,
                                     LocalLastInsertedCoordsId
                                     );
                            if (CoordsBetweenCities != null)
                            {
                                dataGen.Coords.AddRange(CoordsBetweenCities);
                            }
                            else
                            {
                                return CreateData(searchRange, stopTime, pathCount, LocalLastInsertedCoordsId - dataGen.Coords.Count);
                            }
                            dataGen.Paths.Add(path);
                            paths.Add(path);
                            LocalLastInsertedCoordsId = lastInsertedCoordsId + dataGen.Coords.Count;

                        }

                    }

                    WayPointsSchedule wayPoint;
                    Schedule schedule;

                    if (wayPoints.Count == 0)
                    {
                        var departTime = new TimeSpan(r.Next(6, 19), departMinutes[r.Next(0, 4)], 0);

                        dataGen.WayPointsSchedules.Add(wayPoint = new WayPointsSchedule()
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            BusId = instBus.Id,
                            Sequence = j + 1,
                            PathId = path.Id,
                            CityFromDepartTime = departTime,
                            CityToArrivalTime = new TimeSpan((departTime + TimeSpan.FromSeconds(path.Time)).Hours, (departTime + TimeSpan.FromSeconds(path.Time)).Minutes, 0),
                            CityFromDepartTimeInSec = (int)departTime.TotalSeconds,
                            CityToArrivalTimeInSec = (int)new TimeSpan((departTime + TimeSpan.FromSeconds(path.Time)).Hours, (departTime + TimeSpan.FromSeconds(path.Time)).Minutes, 0).TotalSeconds
                        });

                        if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
                        {
                            arrivalDay = (DayOfWeek)(((int)departDay + 1) % 7);
                            differentDays.Add(arrivalDay);
                        }

                        dataGen.Schedules.Add(schedule = new Schedule()
                        {
                            WayPointId = wayPoint.Id,
                            ArrivalDay = arrivalDay,
                            DepartDay = departDay
                        });
                    }
                    else
                    {
                        TimeSpan departTime = wayPoints[j - 1].CityToArrivalTime + TimeSpan.FromMinutes(stopTime); // TimeSpan.FromSeconds(path.Time)
                        var arrivalTime = departTime + TimeSpan.FromSeconds(path.Time);
                        dataGen.WayPointsSchedules.Add(wayPoint = new WayPointsSchedule()
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            BusId = instBus.Id,
                            Sequence = j + 1,
                            PathId = path.Id,
                            CityFromDepartTime = new TimeSpan(departTime.Hours, departTime.Minutes, 0),
                            CityToArrivalTime = new TimeSpan(arrivalTime.Hours, arrivalTime.Minutes, 0),
                            CityFromDepartTimeInSec = (int)new TimeSpan(departTime.Hours, departTime.Minutes, 0).TotalSeconds,
                            CityToArrivalTimeInSec = (int)new TimeSpan(arrivalTime.Hours, arrivalTime.Minutes, 0).TotalSeconds
                        });

                        if (departTime.Days > 0)
                        {
                            departDay = (DayOfWeek)(((int)departDay + 1) % 7);
                        }

                        if (arrivalTime.Days > 0)
                        {
                            arrivalDay = (DayOfWeek)(((int)departDay + 1) % 7);
                            differentDays.Add(arrivalDay);
                        }

                        //if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
                        //{
                        //    arrivalDay = (DayOfWeek)(((int)departDay + 1) % 7);
                        //    differentDays.Add(arrivalDay);
                        //}

                        dataGen.Schedules.Add(schedule = new Schedule()
                        {
                            WayPointId = wayPoint.Id,
                            ArrivalDay = arrivalDay,
                            DepartDay = departDay
                        });
                    }

                    wayPoint.Schedules.Add(schedule);

                    wayPoints.Add(wayPoint);
                }
                switch (differentDays.Count)
                {
                    case 1:
                        for (int j = 0; j < 3; j++)
                        {
                            foreach (var wayPoint in wayPoints)
                            {
                                dataGen.Schedules.Add(new Schedule()
                                {
                                    WayPointId = wayPoint.Id,
                                    ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
                                    DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
                                });
                            }
                        }
                        break;
                    case 2:
                        for (int j = 0; j < 2; j++)
                        {
                            foreach (var wayPoint in wayPoints)
                            {
                                dataGen.Schedules.Add(new Schedule()
                                {
                                    WayPointId = wayPoint.Id,
                                    ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
                                    DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
                                });
                            }
                        }
                        break;
                    case 3:
                        for (int j = 0; j < 1; j++)
                        {
                            foreach (var wayPoint in wayPoints)
                            {
                                dataGen.Schedules.Add(new Schedule()
                                {
                                    WayPointId = wayPoint.Id,
                                    ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
                                    DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
                                });
                            }
                        }
                        break;
                    default:
                        break;
                }

                return dataGen;
            }

            private static List<DataGen> DuplicateData(DataGen prototype,int count)
            {
                return null;
            }





            private static City GetDepCityByType(Capital type)
            {
                Random r = new Random();
                switch (type)
                {
                    case Capital.primary:
                        return capitalsCities[r.Next(0, capitalsCities.Count)];
                    case Capital.admin:
                        return adminCities[r.Next(0, adminCities.Count)];
                    case Capital.minor:
                        return minorCities[r.Next(0, minorCities.Count)];
                    case Capital.none:
                        return commonCities[r.Next(0, commonCities.Count)];
                    default:
                        return new City();
                }
            }
            private static City GetArrCityByType(Capital type, float latFrom, float lngFrom, int wayPointsCount, float extension)
            {
                List<City> sortedCities = new List<City>();
                float limitation = 1f * wayPointsCount;
                Random r = new Random();

                switch (type)
                {
                    case Capital.primary:
                        sortedCities = capitalsCities;
                        break;
                    case Capital.admin:
                        sortedCities = adminCities;
                        break;
                    case Capital.minor:
                        sortedCities = minorCities;
                        break;
                    case Capital.none:
                        sortedCities = commonCities;
                        break;
                    default:
                        break;

                }

                //sortedCities = sortedCities.Where(sc =>
                //    Math.Abs((latFrom + lngFrom) - (sc.Latitude + sc.Longtitude)) < limitation
                //).OrderByDescending(sc =>sc.Latitude).ThenByDescending(sc =>sc.Longtitude).Take(5).ToList();

                sortedCities = sortedCities.Where(sc =>
                    ((sc.Latitude >= latFrom + limitation - extension
                    && sc.Latitude <= latFrom + limitation + extension)
                    ||
                    (sc.Latitude >= latFrom - limitation - extension
                    && sc.Latitude <= latFrom - limitation + extension))
                    &&
                    ((sc.Longtitude >= lngFrom + limitation - extension
                    && sc.Longtitude <= lngFrom + limitation + extension)
                    ||
                    (sc.Longtitude >= lngFrom - limitation - extension
                    && sc.Longtitude <= lngFrom - limitation + extension))
                ).ToList();



                if (sortedCities.Count == 0)
                {
                    return GetArrCityByType(GetDepArrCityType(), latFrom, lngFrom, wayPointsCount, extension * 1.05f);
                }
                else
                {
                    return sortedCities[r.Next(0, sortedCities.Count)];
                }

            }
            private static City GetWayNodeByType(Capital type, float latFrom, float lngFrom, float latDiff, float lngDiff, int iteration, int wayPointsCount,List<City> excludedCities, float extension)
            {
                List<City> sortedCities = new List<City>();
                Random r = new Random();

                switch (type)
                {
                    case Capital.primary:
                        sortedCities = capitalsCities;
                        break;
                    case Capital.admin:
                        sortedCities = adminCities;
                        break;
                    case Capital.minor:
                        sortedCities = minorCities;
                        break;
                    case Capital.none:
                        sortedCities = commonCities;
                        break;
                    default:
                        break;

                }


                sortedCities = sortedCities.Where(sc =>
                    sc.Latitude >= latFrom + (latDiff / wayPointsCount) * (latDiff > 0 ? (iteration - 1) : iteration) - extension * (Math.Abs(lngDiff) / Math.Abs(latDiff))
                    && sc.Latitude <= latFrom + (latDiff / wayPointsCount) * (latDiff > 0 ? iteration : (iteration - 1)) + extension * (Math.Abs(lngDiff) / Math.Abs(latDiff))
                    && sc.Longtitude >= lngFrom + (lngDiff / wayPointsCount) * (lngDiff > 0 ? (iteration - 1) : iteration) - extension * (Math.Abs(latDiff) / Math.Abs(lngDiff))
                    && sc.Longtitude <= lngFrom + (lngDiff / wayPointsCount) * (lngDiff > 0 ? iteration : (iteration - 1)) + extension * (Math.Abs(latDiff) / Math.Abs(lngDiff))
                ).ToList();
                

                if (sortedCities.Count == 0)
                {
                    return GetWayNodeByType(GetDepArrCityType(), latFrom, lngFrom, latDiff, lngDiff, iteration, wayPointsCount, excludedCities,extension * 1.02f);
                }
                else
                {
                    City generatedCity = sortedCities[r.Next(0, sortedCities.Count)];
                    if (excludedCities.Contains(generatedCity))
                    {
                        return GetWayNodeByType(GetDepArrCityType(), latFrom, lngFrom, latDiff, lngDiff, iteration, wayPointsCount, excludedCities, extension * 1.01f);
                    }
                    return generatedCity;
                }
            }
            private static List<Coords> GetWayBetweenCities(ref Path path, City cityFrom, City cityTo, int lastInsertedId)
            {
                List<Coords> coordinates = new List<Coords>();
                //RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<string>() { cityFrom.Name, cityTo.Name });
                RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<Locations>()
                {

                    new Locations(){latLng = new LatLng(){ Lat = cityFrom.Latitude,Lng = cityFrom.Longtitude }},
                    new Locations(){latLng =  new LatLng(){ Lat = cityTo.Latitude, Lng = cityTo.Longtitude }}

                });
                //string jsonString = JsonSerializer.Serialize<RoutingRequest>(routingRequest);
                //var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsJsonAsync(
                    "http://open.mapquestapi.com/directions/v2/route?key=S0B3YTkcDSAWJx7JPKAdw0vw43A67nvH", routingRequest).Result;

                RoutingApiResponseModel responseModel = response.Content.ReadFromJsonAsync<RoutingApiResponseModel>().Result;

                int skipPoints = 1;
                if (responseModel._Route._Shape != null )
                {

                
                    for (int i = 0; i < responseModel._Route._Shape.ShapePoints.Length; i += 2 * skipPoints)
                    {
                        coordinates.Add(new Coords()
                        {
                            Id = lastInsertedId + i / (2 * skipPoints) + 1,
                            PathId = path.Id,
                            Longtitude = responseModel._Route._Shape.ShapePoints[i],
                            Latitude = responseModel._Route._Shape.ShapePoints[i + 1]
                        });

                    }
                    //if (!coordinates.Exists(c => c.Id == lastInsertedId + (responseModel._Route._Shape.ShapePoints.Length - 2) / (2 * skipPoints) + 1))
                    //{
                    //    coordinates.Add(new Coords()
                    //    {
                    //        Id = lastInsertedId + (responseModel._Route._Shape.ShapePoints.Length - 2) / (2 * skipPoints) + 1,
                    //        PathId = path.Id,
                    //        Longtitude = responseModel._Route._Shape.ShapePoints[responseModel._Route._Shape.ShapePoints.Length - 2],
                    //        Latitude = responseModel._Route._Shape.ShapePoints[responseModel._Route._Shape.ShapePoints.Length - 1]
                    //    });
                    //}
                }
                else 
                {

                    return null;
                }
                

                coordinates.OrderBy(c => c.Id).ToList();

                path.Distance = responseModel._Route.Distance;
                path.Time = responseModel._Route.Time;
                path.CoordsCount = responseModel._Route._Shape.ShapePoints.Length;


                //string testjson = await response.Content.ReadAsStringAsync();

                // response.EnsureSuccessStatusCode();
                // var test = response.Headers.Location;

                return coordinates;

            }
            private static Capital GetDepArrCityType()
            {
                Random r = new Random();
                int number = r.Next(0, 101);
                if (number <= 5)
                {
                    return Capital.none;
                }
                else if (number > 5 && number <= 25)
                {
                    return Capital.minor;
                }
                else if (number > 25 && number <= 65)
                {
                    return Capital.admin;
                }
                else
                {
                    return Capital.primary;
                }
            }
            private static Capital GetNodeCityType()
            {
                Random r = new Random();
                int number = r.Next(0, 101);
                if (number <= 40)
                {
                    return Capital.none;
                }
                else if (number > 40 && number <= 75)
                {
                    return Capital.minor;
                }
                else if (number > 75 && number <= 95)
                {
                    return Capital.admin;
                }
                else
                {
                    return Capital.primary;
                }
            }
            private class DataGen
            {
                public Bus Bus { get; set; }
                public List<WayPointsSchedule> WayPointsSchedules { get; set; }
                public List<Coords> Coords { get; set; }
                public List<Path> Paths { get; set; }
                public List<Schedule> Schedules { get; set; }

                public DataGen()
                {
                    WayPointsSchedules = new List<WayPointsSchedule>();
                    Coords = new List<Coords>();
                    Paths = new List<Path>();
                    Schedules = new List<Schedule>();
                }
            }

            

        }

        //public void GenerateDB(float searchRange = 0.5f, int stopTime = 15, int busCountGen = 1)
        //{
        //    Random r = new Random();         
        //    int[] departMinutes = new int[4] { 0, 15, 30, 45 };

        //    List<Coords> coords = new List<Coords>();
        //    List<Path> allPaths = new List<Path>();

        //    using (var context = new MapAppContext())
        //    {
        //        List<City> capitalsCities = context.Cities.Where(c => c.Capital == Capital.primary).ToList();
        //        List<City> adminCities = context.Cities.Where(c => c.Capital == Capital.admin).ToList();
        //        List<City> minorCities = context.Cities.Where(c => c.Capital == Capital.minor).ToList();
        //        List<City> commonCities = context.Cities.Where(c => c.Capital == Capital.none).ToList();
                           
        //        int lastInsertedId;
        //        if (context.Coords.OrderBy(p => p.Id).LastOrDefault() != null)
        //        {
        //            lastInsertedId = context.Coords.OrderBy(p => p.Id).LastOrDefault().Id;
        //        }
        //        else
        //        {
        //            lastInsertedId = 1;
        //        }

        //        for (int i = 0; i < busCountGen; i++)
        //        {
        //            Bus instBus = new Bus()
        //            {
        //                Operator = "Rand " + i,
        //            };
        //            context.Buses.Add(instBus);

        //            int pathCount = 12;


        //            City startCity = GetDepCityByType(GetDepArrCityType());
        //            City endCity = GetArrCityByType(GetDepArrCityType(), startCity.Latitude, startCity.Longtitude, pathCount,searchRange * 2);

        //            float latDiff = endCity.Latitude - startCity.Latitude,
        //                  lngDiff = endCity.Longtitude - startCity.Longtitude; 

        //            City city1 = GetDepCityByType(GetDepArrCityType());
        //            City city2 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude,latDiff,lngDiff, 1,pathCount, searchRange);

        //            List<Path> paths = new List<Path>();
        //            List<WayPointsSchedule> wayPoints = new List<WayPointsSchedule>();
        //            List<DayOfWeek> differentDays = new List<DayOfWeek>();
        //            DayOfWeek departDay = (DayOfWeek)r.Next(0, 7);
        //            DayOfWeek arrivalDay = departDay;
        //            differentDays.Add(departDay);

                    
                    

        //            for (int j = 0; j < pathCount; j++)
        //            {
        //                Path path = new Path();
                        
        //                int syclesCount = 0;
                        
        //                if (j == pathCount - 1)
        //                {
        //                    if (j % 2 == 0)
        //                    {
        //                        city2 = endCity;

        //                        var check = context.Paths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList();
        //                        check.AddRange(allPaths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList());


        //                        if (check.Count > 0)
        //                        {
        //                            paths.Add(check[0]);
        //                            path = check[0];
        //                        }
        //                        else
        //                        {
        //                            context.Paths.Add(path = new Path()
        //                            {
        //                                CityFromId = city1.Id,
        //                                CityToId = city2.Id
        //                            });
        //                            coords.AddRange(GetWayBetweenCities
        //                                (path,
        //                                context.Cities.Where(c => c.Id == path.CityFromId).First(),
        //                                context.Cities.Where(c => c.Id == path.CityToId).First(),
        //                                lastInsertedId,
        //                                context
        //                                ));

        //                            lastInsertedId = lastInsertedId + coords.Count;
        //                            paths.Add(path);
        //                            allPaths.Add(path);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        city1 = endCity;

        //                        var check = context.Paths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList();
        //                        check.AddRange(allPaths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList());

        //                        if (check.Count > 0)
        //                        {
        //                            paths.Add(check[0]);
        //                            path = check[0];
        //                        }
        //                        else
        //                        {
        //                            context.Paths.Add(path = new Path()
        //                            {
        //                                CityFromId = city2.Id,
        //                                CityToId = city1.Id,
        //                            });
        //                            coords.AddRange(GetWayBetweenCities
        //                                (path,
        //                                context.Cities.Where(c => c.Id == path.CityFromId).First(),
        //                                context.Cities.Where(c => c.Id == path.CityToId).First(),
        //                                lastInsertedId,
        //                                context
        //                                ));
        //                            lastInsertedId = lastInsertedId + coords.Count;
        //                            paths.Add(path);
        //                            allPaths.Add(path);
        //                        }
        //                    }
        //                }
        //                else if (j % 2 == 0)
        //                {
        //                    while (paths.Where(p => p.CityToId == city2.Id).ToList().Count > 0
        //                        || paths.Where(p => p.CityFromId == city2.Id).ToList().Count > 0)
        //                    {
        //                        city2 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, j + 1, pathCount, searchRange);

        //                        syclesCount++;
        //                        if (syclesCount > 1000)
        //                        {
        //                            city2 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, j + 1, pathCount, searchRange * 2);
        //                            if (syclesCount > 2000)
        //                            {
        //                                break;
        //                            }
        //                        }
        //                    }

        //                    if (syclesCount > 2000)
        //                    {
        //                        break;
        //                    }

        //                    var check = context.Paths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList();
        //                    check.AddRange(allPaths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList());


        //                    if (check.Count > 0)
        //                    {
        //                        paths.Add(check[0]);
        //                        path = check[0];
        //                    }
        //                    else
        //                    {
        //                        context.Paths.Add(path = new Path()
        //                        {
        //                            CityFromId = city1.Id,
        //                            CityToId = city2.Id
        //                        });
        //                        coords.AddRange(GetWayBetweenCities
        //                            (path,
        //                            context.Cities.Where(c => c.Id == path.CityFromId).First(),
        //                            context.Cities.Where(c => c.Id == path.CityToId).First(),
        //                            lastInsertedId,
        //                            context
        //                            ));
                                
        //                        lastInsertedId = lastInsertedId + coords.Count;
        //                        paths.Add(path);
        //                        allPaths.Add(path);
        //                    }

        //                }
        //                else
        //                {

        //                    while (paths.Where(p => p.CityFromId == city1.Id).ToList().Count > 0
        //                       || paths.Where(p => p.CityToId == city1.Id).ToList().Count > 0)
        //                    {
        //                        city1 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, j + 1, pathCount, searchRange);

        //                        syclesCount++;
        //                        if (syclesCount > 1000)
        //                        {
        //                            city1 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, j + 1, pathCount, searchRange * 2);
        //                            if (syclesCount > 2000)
        //                            {
        //                                break;
        //                            }
        //                        }
        //                    }

        //                    if (syclesCount > 2000)
        //                    {
        //                        break;
        //                    }

        //                    var check = context.Paths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList();
        //                    check.AddRange(allPaths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList());

        //                    if (check.Count > 0)
        //                    {
        //                        paths.Add(check[0]);
        //                        path = check[0];
        //                    }
        //                    else
        //                    {
        //                        context.Paths.Add(path = new Path()
        //                        {
        //                            CityFromId = city2.Id,
        //                            CityToId = city1.Id,
        //                        });
        //                        coords.AddRange(GetWayBetweenCities
        //                            (path,
        //                            context.Cities.Where(c => c.Id == path.CityFromId).First(),
        //                            context.Cities.Where(c => c.Id == path.CityToId).First(), 
        //                            lastInsertedId,
        //                            context
        //                            ));
        //                        lastInsertedId = lastInsertedId + coords.Count;
        //                        paths.Add(path);
        //                        allPaths.Add(path);
        //                    }

        //                }

        //                WayPointsSchedule wayPoint;
        //                Schedule schedule;
                        
        //                if (wayPoints.Count == 0)
        //                {
        //                    var departTime = new TimeSpan(r.Next(6, 19), departMinutes[r.Next(0, 4)], 0);
                            
        //                    context.WayPointsSchedules.Add(wayPoint = new WayPointsSchedule()
        //                    {
        //                        BusId = instBus.Id,
        //                        Sequence = j + 1,
        //                        PathId = path.Id,
        //                        CityFromDepartTime = departTime,
        //                        CityToArrivalTime = new TimeSpan((departTime + TimeSpan.FromSeconds(path.Time)).Hours, (departTime + TimeSpan.FromSeconds(path.Time)).Minutes,0),
        //                        CityFromDepartTimeInSec = (int)departTime.TotalSeconds,
        //                        CityToArrivalTimeInSec = (int)new TimeSpan((departTime + TimeSpan.FromSeconds(path.Time)).Hours, (departTime + TimeSpan.FromSeconds(path.Time)).Minutes, 0).TotalSeconds
        //                    });

        //                    if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
        //                    {
        //                        arrivalDay = (DayOfWeek)(((int)departDay + 1) % 7);
        //                        differentDays.Add(arrivalDay);
        //                    }

        //                    context.Schedules.Add(schedule = new Schedule()
        //                    {
        //                        WayPointId = wayPoint.Id,
        //                        ArrivalDay = arrivalDay,
        //                        DepartDay = departDay
        //                    });
        //                }
        //                else
        //                {
        //                    TimeSpan departTime = wayPoints[j - 1].CityToArrivalTime + TimeSpan.FromMinutes(stopTime); // TimeSpan.FromSeconds(path.Time)
        //                    var arrivalTime = departTime + TimeSpan.FromSeconds(path.Time);
        //                    context.WayPointsSchedules.Add(wayPoint = new WayPointsSchedule()
        //                    {
        //                        BusId = instBus.Id,
        //                        Sequence = j + 1,
        //                        PathId = path.Id,
        //                        CityFromDepartTime = new TimeSpan(departTime.Hours, departTime.Minutes , 0),
        //                        CityToArrivalTime = new TimeSpan(arrivalTime.Hours, arrivalTime.Minutes, 0),
        //                        CityFromDepartTimeInSec = (int)new TimeSpan(departTime.Hours, departTime.Minutes, 0).TotalSeconds,
        //                        CityToArrivalTimeInSec = (int)new TimeSpan(arrivalTime.Hours, arrivalTime.Minutes, 0).TotalSeconds
        //                    });

        //                    if (departTime.Days > 0)
        //                    {
        //                        departDay = (DayOfWeek)(((int)departDay + 1) % 7);
        //                        differentDays.Add(arrivalDay);
        //                    }

        //                    if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
        //                    {
        //                        arrivalDay = (DayOfWeek)(((int)departDay + 1) % 7);
        //                        differentDays.Add(arrivalDay);
        //                    }

        //                    context.Schedules.Add(schedule = new Schedule()
        //                    {
        //                        WayPointId = wayPoint.Id,
        //                        ArrivalDay = arrivalDay,
        //                        DepartDay = departDay
        //                    });

        //                    if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
        //                    {
        //                        departDay = arrivalDay;
        //                    }

        //                    if (departTime.Days > 0)
        //                    {
        //                        arrivalDay = departDay;
        //                    }
        //                }

        //                wayPoint.Schedules.Add(schedule);

        //                wayPoints.Add(wayPoint);
        //            }
        //            switch (differentDays.Count)
        //            {
        //                case 1:
        //                    for (int j = 0; j < 3; j++)
        //                    {
        //                        foreach (var wayPoint in wayPoints)
        //                        {
        //                            context.Schedules.Add(new Schedule()
        //                            {
        //                                WayPointId = wayPoint.Id,
        //                                ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
        //                                DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
        //                            });
        //                        }
        //                    }
        //                    break;
        //                case 2:
        //                    for (int j = 0; j < 2; j++)
        //                    {
        //                        foreach (var wayPoint in wayPoints)
        //                        {
        //                            context.Schedules.Add(new Schedule()
        //                            {
        //                                WayPointId = wayPoint.Id,
        //                                ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
        //                                DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
        //                            });
        //                        }
        //                    }
        //                    break;
        //                case 3:
        //                    for (int j = 0; j < 1; j++)
        //                    {
        //                        foreach (var wayPoint in wayPoints)
        //                        {
        //                            context.Schedules.Add(new Schedule()
        //                            {
        //                                WayPointId = wayPoint.Id,
        //                                ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
        //                                DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
        //                            });
        //                        }
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }
                   
        //        }
        //        context.Coords.AddRange(coords);
        //        context.Database.OpenConnection();
        //        try
        //        {
        //            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Coords ON;");
        //            context.SaveChanges();
        //            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Coords OFF;");
        //        }
        //        finally
        //        {
        //            context.Database.CloseConnection();
        //        }

        //        City GetDepCityByType(Capital type)
        //        {
        //            switch (type)
        //            {
        //                case Capital.primary:
        //                    return capitalsCities[r.Next(0, capitalsCities.Count)];
        //                case Capital.admin:
        //                    return adminCities[r.Next(0, adminCities.Count)];
        //                case Capital.minor:
        //                    return minorCities[r.Next(0, minorCities.Count)];
        //                case Capital.none:
        //                    return commonCities[r.Next(0, commonCities.Count)];
        //                default:
        //                    return new City();
        //            }
        //        }

        //        City GetArrCityByType(Capital type,float latFrom, float lngFrom,int wayPointsCount,float extension)
        //        {
        //            List<City> sortedCities = new List<City>();
        //            float limitation = 1f * wayPointsCount;

        //            switch (type)
        //            {
        //                case Capital.primary:
        //                    sortedCities = capitalsCities;
        //                    break;
        //                case Capital.admin:
        //                    sortedCities = adminCities;
        //                    break;
        //                case Capital.minor:
        //                    sortedCities = minorCities;
        //                    break;
        //                case Capital.none:
        //                    sortedCities = commonCities;
        //                    break;
        //                default:
        //                    break;

        //            }

        //            //sortedCities = sortedCities.Where(sc =>
        //            //    Math.Abs((latFrom + lngFrom) - (sc.Latitude + sc.Longtitude)) < limitation
        //            //).OrderByDescending(sc =>sc.Latitude).ThenByDescending(sc =>sc.Longtitude).Take(5).ToList();

        //            sortedCities = sortedCities.Where(sc =>
        //                ((sc.Latitude >= latFrom + limitation - extension
        //                && sc.Latitude <= latFrom + limitation + extension) 
        //                || 
        //                (sc.Latitude >= latFrom - limitation - extension
        //                && sc.Latitude <= latFrom - limitation + extension))
        //                &&
        //                ((sc.Longtitude >= lngFrom + limitation - extension
        //                && sc.Longtitude <= lngFrom + limitation + extension)
        //                || 
        //                (sc.Longtitude >= lngFrom - limitation - extension
        //                && sc.Longtitude <= lngFrom - limitation + extension))
        //            ).ToList();



        //            if (sortedCities.Count == 0)
        //            {
        //                return GetArrCityByType(GetDepArrCityType(), latFrom, lngFrom, wayPointsCount, extension * 1.05f);
        //            }
        //            else
        //            {
        //                return sortedCities[r.Next(0, sortedCities.Count)];
        //            }
                    
        //        }

        //        City GetWayNodeByType(Capital type,float latFrom, float lngFrom, float latDiff, float lngDiff, int iteration,int wayPointsCount, float extension)
        //        {
        //            List<City> sortedCities = new List<City>();


        //            switch (type)
        //            {
        //                case Capital.primary:
        //                    sortedCities = capitalsCities;
        //                    break;
        //                case Capital.admin:
        //                    sortedCities = adminCities;
        //                    break;
        //                case Capital.minor:
        //                    sortedCities = minorCities;
        //                    break;
        //                case Capital.none:
        //                    sortedCities = commonCities;
        //                    break;
        //                default:
        //                    break;

        //            }

        //            if (Math.Abs(latDiff) < Math.Abs(lngDiff))
        //            {
        //                sortedCities = sortedCities.Where(sc =>
        //                    sc.Latitude >= latFrom + (latDiff / wayPointsCount) * (latDiff > 0 ? (iteration - 1) : iteration) - extension * (Math.Abs(lngDiff)/ Math.Abs(latDiff))
        //                    && sc.Latitude <= latFrom + (latDiff / wayPointsCount) * (latDiff > 0 ? iteration : (iteration - 1)) + extension * (Math.Abs(lngDiff) / Math.Abs(latDiff))
        //                    && sc.Longtitude >= lngFrom + (lngDiff / wayPointsCount) * (lngDiff > 0 ? (iteration - 1) : iteration) - extension * (Math.Abs(latDiff) / Math.Abs(lngDiff))
        //                    && sc.Longtitude <= lngFrom + (lngDiff / wayPointsCount) * (lngDiff > 0 ? iteration : (iteration - 1)) + extension * (Math.Abs(latDiff) / Math.Abs(lngDiff))
        //                ).ToList();
        //            }
        //            else
        //            {
        //                sortedCities = sortedCities.Where(sc =>
        //                    sc.Latitude >= latFrom + (latDiff / wayPointsCount) * (latDiff > 0 ? (iteration - 1) : iteration) - extension * (Math.Abs(lngDiff) / Math.Abs(latDiff))
        //                    && sc.Latitude <= latFrom + (latDiff / wayPointsCount) * (latDiff > 0 ? iteration : (iteration - 1)) + extension * (Math.Abs(lngDiff) / Math.Abs(latDiff))
        //                    && sc.Longtitude >= lngFrom + (lngDiff / wayPointsCount) * (lngDiff > 0 ? (iteration - 1) : iteration) - extension * (Math.Abs(latDiff) / Math.Abs(lngDiff))
        //                    && sc.Longtitude <= lngFrom + (lngDiff / wayPointsCount) * (lngDiff > 0 ? iteration : (iteration - 1)) + extension * (Math.Abs(latDiff) / Math.Abs(lngDiff))
        //                ).ToList();
        //            }

        //            if (sortedCities.Count == 0)
        //            {
        //                return GetWayNodeByType(GetDepArrCityType(), latFrom, lngFrom, latDiff, lngDiff,iteration, wayPointsCount, extension * 1.02f);
        //            }
        //            else
        //            {
        //                return sortedCities[r.Next(0, sortedCities.Count)];
        //            }
        //        }


        //    }

        //    List<Coords> GetWayBetweenCities(Path path, City cityFrom, City cityTo, int lastInsertedId, MapAppContext context)
        //    {
        //        List<Coords> coordinates = new List<Coords>();
        //        //RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<string>() { cityFrom.Name, cityTo.Name });
        //        RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<Locations>()
        //        {
                    
        //            new Locations(){latLng = new LatLng(){ Lat = cityFrom.Latitude,Lng = cityFrom.Longtitude }},
        //            new Locations(){latLng =  new LatLng(){ Lat = cityTo.Latitude, Lng = cityTo.Longtitude }}
                            
        //        });
        //        //string jsonString = JsonSerializer.Serialize<RoutingRequest>(routingRequest);
        //        //var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        //        HttpResponseMessage response = client.PostAsJsonAsync(
        //            "http://open.mapquestapi.com/directions/v2/route?key=S0B3YTkcDSAWJx7JPKAdw0vw43A67nvH", routingRequest).Result;

        //        RoutingApiResponseModel responseModel = response.Content.ReadFromJsonAsync<RoutingApiResponseModel>().Result;

        //        int skipPoints = 1;

        //        for (int i = 0; i < responseModel._Route._Shape.ShapePoints.Length; i += 2 * skipPoints)
        //        {
        //            coordinates.Add(new Coords()
        //            {
        //                Id = lastInsertedId + i/(2 * skipPoints) + 1,
        //                PathId = path.Id,
        //                Longtitude = responseModel._Route._Shape.ShapePoints[i],
        //                Latitude = responseModel._Route._Shape.ShapePoints[i + 1]
        //            });

        //        }
        //        if (!coordinates.Exists(c => c.Id == lastInsertedId + (responseModel._Route._Shape.ShapePoints.Length - 2) / (2 * skipPoints) + 1))
        //        {
        //            coordinates.Add(new Coords()
        //            {
        //                Id = lastInsertedId + (responseModel._Route._Shape.ShapePoints.Length - 2) / (2 * skipPoints) + 1,
        //                PathId = path.Id,
        //                Longtitude = responseModel._Route._Shape.ShapePoints[responseModel._Route._Shape.ShapePoints.Length - 2],
        //                Latitude = responseModel._Route._Shape.ShapePoints[responseModel._Route._Shape.ShapePoints.Length - 1]
        //            });
        //        }
               
        //        coordinates = coordinates.OrderBy(c => c.Id).ToList();

        //        cityFrom.Longtitude = coordinates.First().Latitude;
        //        cityFrom.Latitude = coordinates.First().Longtitude;

        //        cityTo.Longtitude = coordinates.Last().Latitude;
        //        cityTo.Latitude = coordinates.Last().Longtitude;

        //        path.Distance = responseModel._Route.Distance;
        //        path.Time = responseModel._Route.Time;

        //        context.Cities.UpdateRange(cityFrom,cityTo);

        //        //string testjson = await response.Content.ReadAsStringAsync();

        //        // response.EnsureSuccessStatusCode();
        //        // var test = response.Headers.Location;

        //        return coordinates;

        //    }

        //    Capital GetDepArrCityType()
        //    {
        //        int number = r.Next(0,101);
        //        if (number <= 5)
        //        {
        //            return Capital.none;
        //        }
        //        else if (number > 5 && number <= 25)
        //        {
        //            return Capital.minor;
        //        }
        //        else if (number > 25 && number <= 65)
        //        {
        //            return Capital.admin;
        //        }
        //        else
        //        {
        //            return Capital.primary;
        //        }
        //    }

        //    Capital GetNodeCityType()
        //    {
        //        int number = r.Next(0, 101);
        //        if (number <= 40)
        //        {
        //            return Capital.none;
        //        }
        //        else if (number > 40 && number <= 75)
        //        {
        //            return Capital.minor;
        //        }
        //        else if (number > 75 && number <= 95)
        //        {
        //            return Capital.admin;
        //        }
        //        else
        //        {
        //            return Capital.primary;
        //        }
        //    }
        //}

        
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
