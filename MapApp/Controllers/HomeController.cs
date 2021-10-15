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
            //GenerateDB();
            return View();
        }
        //public JsonResult GetFullRoute(string busId)
        //{
        //    PathQuery fullRoute = new PathQuery();
        //    using (var context = new MapAppContext())
        //    {
        //        var fullRoutePathIds = (from bus in context.Buses
        //                            join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
        //                            join path in context.Paths on waypoint.PathId equals path.Id
        //                            where bus.Id == busId
        //                            orderby waypoint.Sequence
        //                            select new
        //                            {
        //                                path.Id
        //                            }).ToList();

        //        var isExistsArr = new List<int>();
        //        var cacheResult = (Dictionary<string, PathQuery>)cache.Get("Paths");
        //        for (int i = 0; i < fullRoutePathIds.Count; i++)
        //        {
        //            if (cacheResult.ContainsKey(fullRoutePathIds[i].ToString()))
        //            {
        //                isExistsArr.Add(i + 1);
        //            }

        //        }

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
                                          where bus.Id == busId && waypoint.Sequence == sequence
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
                var routes = (from bus in context.Buses
                              join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                              join path in context.Paths on waypoint.PathId equals path.Id
                              join city in context.Cities on path.CityFromId equals city.Id
                              //join seconds in (from innerwaypoint in context.WayPointsSchedules
                              //                 select new
                              //                 {
                              //                     WayPointId = innerwaypoint.Id,
                              //                     CityFromDepartTimeInSeconds = innerwaypoint.CityFromDepartTime.Seconds
                              //                                                  + innerwaypoint.CityFromDepartTime.Minutes * 60
                              //                                                  + innerwaypoint.CityFromDepartTime.Hours * 3600,
                              //                     CityToArrivalTimeInSeconds = innerwaypoint.CityToArrivalTime.Seconds
                              //                                                      + innerwaypoint.CityToArrivalTime.Minutes * 60
                              //                                                      + innerwaypoint.CityToArrivalTime.Hours * 3600
                              //                 }) on waypoint.Id equals seconds.WayPointId
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
                                 //&& (seconds.CityFromDepartTimeInSeconds > seconds.CityToArrivalTimeInSeconds
                                 //    ? (time.TotalSeconds + 86400 >= seconds.CityFromDepartTimeInSeconds) && (time.TotalSeconds + 86400 <= seconds.CityToArrivalTimeInSeconds + 86400)
                                 //    : (time.TotalSeconds >= seconds.CityFromDepartTimeInSeconds) && (time.TotalSeconds <= seconds.CityToArrivalTimeInSeconds))
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

        public JsonResult GetAllCities()
        {
            using (var context = new MapAppContext())
            {
                var cities = context.Cities.ToList();
                return Json(cities);
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

        public void GenerateDB()
        {
            Random r = new Random();
            int citiesCount = 90;
            double searchRange = 4.5;
            int stopTime = 15;
            int[] departMinutes = new int[4] { 0, 15, 30, 45 };
            int busCountGen = 100;

            using (var context = new MapAppContext())
            {
                List<Coords> coords = new List<Coords>();
               
      

                int lastInsertedId;
                if (context.Coords.OrderBy(p => p.Id).LastOrDefault() != null)
                {
                    lastInsertedId = context.Coords.OrderBy(p => p.Id).LastOrDefault().Id;
                }
                else
                {
                    lastInsertedId = 1;
                }

                List<Path> allPaths = new List<Path>();

                for (int i = 0; i < busCountGen; i++)
                {
                    Bus instBus = new Bus()
                    {
                        Operator = "Rand " + i,
                    };
                    context.Buses.Add(instBus);

                    int rand1 = r.Next(1, citiesCount);
                    int rand2 = r.Next(1, citiesCount);
                    City city1 = context.Cities.Where(c => c.Id == rand1.ToString()).First();
                    City city2 = context.Cities.Where(c => c.Id == rand2.ToString()).First();
                    List<Path> paths = new List<Path>();
                    List<WayPointsSchedule> wayPoints = new List<WayPointsSchedule>();
                    List<DayOfWeek> differentDays = new List<DayOfWeek>();
                    DayOfWeek departDay = (DayOfWeek)r.Next(0, 7);
                    DayOfWeek arrivalDay = departDay;
                    differentDays.Add(departDay);

                    int pathCount = 6;

                    for (int j = 0; j < pathCount; j++)
                    {
                        Path path = new Path();
                        
                        int syclesCount = 0;
                        if (j % 2 == 0)
                        {
                            rand2 = r.Next(1, citiesCount);
                            city2 = context.Cities.Where(c => c.Id == rand2.ToString()).First();

                            while (paths.Where(p => p.CityToId == rand2.ToString() ).ToList().Count > 0
                                || paths.Where(p => p.CityFromId == rand2.ToString()).ToList().Count > 0
                            || rand1 == rand2
                            || (Math.Abs(city2.Latitude - city1.Latitude) > searchRange 
                            || Math.Abs(city2.Longtitude - city1.Longtitude) > searchRange)) //&& (Math.Abs(city2.Longtitude + city2.Latitude) - Math.Abs(city1.Latitude + city1.Longtitude)) > searchRange)
                            {
                                rand2 = r.Next(1, citiesCount);
                                city2 = context.Cities.Where(c => c.Id == rand2.ToString()).First();

                                syclesCount++;
                                if (syclesCount > 1000)
                                {
                                    break;
                                }
                            }

                            if (syclesCount > 1000)
                            {
                                break;
                            }

                            var check = context.Paths.Where(p => p.CityFromId == rand1.ToString() && p.CityToId == rand2.ToString()).ToList();
                            check.AddRange(allPaths.Where(p => p.CityFromId == rand1.ToString() && p.CityToId == rand2.ToString()).ToList());


                            if (check.Count > 0)
                            {
                                paths.Add(check[0]);
                                path = check[0];
                            }
                            else
                            {
                                context.Paths.Add(path = new Path()
                                {
                                    CityFromId = rand1.ToString(),
                                    CityToId = rand2.ToString()
                                });
                                coords.AddRange(GetWayBetweenCities
                                    (path,
                                    context.Cities.Where(c => c.Id == path.CityFromId).First(),
                                    context.Cities.Where(c => c.Id == path.CityToId).First(),
                                    lastInsertedId,
                                    context
                                    ));
                                
                                lastInsertedId = lastInsertedId + coords.Count;
                                paths.Add(path);
                                allPaths.Add(path);
                            }

                        }
                        else
                        {
                            rand1 = r.Next(1, citiesCount);
                            city1 = context.Cities.Where(c => c.Id == rand1.ToString()).First();
                            while (paths.Where(p => p.CityFromId == rand1.ToString()).ToList().Count > 0
                               || paths.Where(p => p.CityToId == rand1.ToString()).ToList().Count > 0
                            || rand1 == rand2
                            || (Math.Abs(city1.Latitude - city2.Latitude) > searchRange 
                            || Math.Abs(city1.Longtitude - city2.Longtitude) > searchRange)) //&& (Math.Abs(city1.Longtitude + city1.Latitude) - Math.Abs(city2.Latitude + city2.Longtitude)) > searchRange)//??
                            {
                                rand1 = r.Next(1, citiesCount);
                                city1 = context.Cities.Where(c => c.Id == rand1.ToString()).First();

                                syclesCount++;
                                if (syclesCount > 1000)
                                {
                                    break;
                                }
                            }

                            if (syclesCount > 1000)
                            {
                                break;
                            }

                            var check = context.Paths.Where(p => p.CityFromId == rand2.ToString() && p.CityToId == rand1.ToString()).ToList();
                            check.AddRange(allPaths.Where(p => p.CityFromId == rand2.ToString() && p.CityToId == rand1.ToString()).ToList());

                            if (check.Count > 0)
                            {
                                paths.Add(check[0]);
                                path = check[0];
                            }
                            else
                            {
                                context.Paths.Add(path = new Path()
                                {
                                    CityFromId = rand2.ToString(),
                                    CityToId = rand1.ToString(),
                                });
                                coords.AddRange(GetWayBetweenCities
                                    (path,
                                    context.Cities.Where(c => c.Id == path.CityFromId).First(),
                                    context.Cities.Where(c => c.Id == path.CityToId).First(), 
                                    lastInsertedId,
                                    context
                                    ));
                                lastInsertedId = lastInsertedId + coords.Count;
                                paths.Add(path);
                                allPaths.Add(path);
                            }

                        }

                        WayPointsSchedule wayPoint;
                        Schedule schedule;
                        
                        if (wayPoints.Count == 0)
                        {
                            var departTime = new TimeSpan(r.Next(6, 19), departMinutes[r.Next(0, 4)], 0);
                            
                            context.WayPointsSchedules.Add(wayPoint = new WayPointsSchedule()
                            {
                                BusId = instBus.Id,
                                Sequence = j + 1,
                                PathId = path.Id,
                                CityFromDepartTime = departTime,
                                CityToArrivalTime = new TimeSpan((departTime + TimeSpan.FromSeconds(path.Time)).Hours, (departTime + TimeSpan.FromSeconds(path.Time)).Minutes,0),
                                CityFromDepartTimeInSec = (int)departTime.TotalSeconds,
                                CityToArrivalTimeInSec = (int)new TimeSpan((departTime + TimeSpan.FromSeconds(path.Time)).Hours, (departTime + TimeSpan.FromSeconds(path.Time)).Minutes, 0).TotalSeconds
                            });

                            if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
                            {
                                arrivalDay = (DayOfWeek)(((int)departDay + 1) % 6);
                                differentDays.Add(arrivalDay);
                            }

                            context.Schedules.Add(schedule = new Schedule()
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
                            context.WayPointsSchedules.Add(wayPoint = new WayPointsSchedule()
                            {
                                BusId = instBus.Id,
                                Sequence = j + 1,
                                PathId = path.Id,
                                CityFromDepartTime = new TimeSpan(departTime.Hours, departTime.Minutes , 0),
                                CityToArrivalTime = new TimeSpan(arrivalTime.Hours, arrivalTime.Minutes, 0),
                                CityFromDepartTimeInSec = (int)new TimeSpan(departTime.Hours, departTime.Minutes, 0).TotalSeconds,
                                CityToArrivalTimeInSec = (int)new TimeSpan(arrivalTime.Hours, arrivalTime.Minutes, 0).TotalSeconds
                            });

                            if (departTime.Days > 0)
                            {
                                departDay = (DayOfWeek)(((int)departDay + 1) % 6);
                                differentDays.Add(arrivalDay);
                            }

                            if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
                            {
                                arrivalDay = (DayOfWeek)(((int)departDay + 1) % 6);
                                differentDays.Add(arrivalDay);
                            }

                            context.Schedules.Add(schedule = new Schedule()
                            {
                                WayPointId = wayPoint.Id,
                                ArrivalDay = arrivalDay,
                                DepartDay = departDay
                            });

                            if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
                            {
                                departDay = arrivalDay;
                            }

                            if (departTime.Days > 0)
                            {
                                arrivalDay = departDay;
                            }
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
                                    context.Schedules.Add(new Schedule()
                                    {
                                        WayPointId = wayPoint.Id,
                                        ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 6),
                                        DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 6)
                                    });
                                }
                            }
                            break;
                        case 2:
                            for (int j = 0; j < 2; j++)
                            {
                                foreach (var wayPoint in wayPoints)
                                {
                                    context.Schedules.Add(new Schedule()
                                    {
                                        WayPointId = wayPoint.Id,
                                        ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 6),
                                        DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 6)
                                    });
                                }
                            }
                            break;
                        case 3:
                            for (int j = 0; j < 1; j++)
                            {
                                foreach (var wayPoint in wayPoints)
                                {
                                    context.Schedules.Add(new Schedule()
                                    {
                                        WayPointId = wayPoint.Id,
                                        ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 6),
                                        DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 6)
                                    });
                                }
                            }
                            break;
                        default:
                            break;
                    }
                   
                }
                context.Coords.AddRange(coords);
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

            List<Coords> GetWayBetweenCities(Path path, City cityFrom, City cityTo, int lastInsertedId, MapAppContext context)
            {
                List<Coords> coordinates = new List<Coords>();
                RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<string>() { cityFrom.Name, cityTo.Name });

                //string jsonString = JsonSerializer.Serialize<RoutingRequest>(routingRequest);
                //var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsJsonAsync(
                    "http://open.mapquestapi.com/directions/v2/route?key=S0B3YTkcDSAWJx7JPKAdw0vw43A67nvH", routingRequest).Result;

                RoutingApiResponseModel responseModel = response.Content.ReadFromJsonAsync<RoutingApiResponseModel>().Result;


                for (int i = 0; i < responseModel._Route._Shape.ShapePoints.Length; i += 2)
                {
                    coordinates.Add(new Coords()
                    {
                        Id = lastInsertedId + i/2 + 1,
                        PathId = path.Id,
                        Longtitude = responseModel._Route._Shape.ShapePoints[i],
                        Latitude = responseModel._Route._Shape.ShapePoints[i + 1]
                    });

                }
                coordinates = coordinates.OrderBy(c => c.Id).ToList();

                cityFrom.Longtitude = coordinates.First().Latitude;
                cityFrom.Latitude = coordinates.First().Longtitude;

                cityTo.Longtitude = coordinates.Last().Latitude;
                cityTo.Latitude = coordinates.Last().Longtitude;

                path.Distance = responseModel._Route.Distance;
                path.Time = responseModel._Route.Time;

                context.Cities.UpdateRange(cityFrom,cityTo);

                //string testjson = await response.Content.ReadAsStringAsync();

                // response.EnsureSuccessStatusCode();
                // var test = response.Headers.Location;

                return coordinates;

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
