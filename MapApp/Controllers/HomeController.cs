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

        public JsonResult GetPath(string busId)
        {

            IEnumerable<float[]> busPath;
            Dictionary<string, IEnumerable<float[]>> cacheResult = (Dictionary<string, IEnumerable<float[]>>)cache.Get("Paths") ?? new Dictionary<string, IEnumerable<float[]>>();

            if (cacheResult.ContainsKey(busId))
            {
                busPath = cacheResult[busId];
                return Json(busPath);
            }
            else
            {
                using (var context = new MapAppContext())
                {
                    busPath = (from bus in context.Buses
                               join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                               join path in context.Paths on waypoint.PathId equals path.Id
                               join coords in context.Coords on path.Id equals coords.PathId
                               where bus.Id == busId
                               orderby waypoint.Sequence, coords.Id
                               select new float[2]
                               {
                                   coords.Longtitude,
                                   coords.Latitude
                               }).ToArray();
                   
                    cacheResult.Add(busId,busPath);
                                       
                    cache.Set("Paths", cacheResult, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(100)
                    });
                    return Json(busPath);
                }
            }
           
        }
        public JsonResult GetVisibleRoutes(float topLat,float topLong,float botLat,float botLong,DateTime dateTime, List<string> existingRoutes)
        {
            int correction = 2;
            using (var context = new MapAppContext())
            {
                var routes = (from bus in context.Buses
                              join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                              join path in context.Paths on waypoint.PathId equals path.Id
                              join city in context.Cities on path.CityFromId equals city.Id
                              //join schedule in context.Schedules on bus.Id equals schedule.BusId
                              where !existingRoutes.Contains(bus.Id)
                                 && (city.Latitude >= botLat - correction && city.Longtitude >= botLong - correction)
                                 && (city.Latitude <= topLat + correction && city.Longtitude <= topLong + correction)
                              select new 
                              {
                                  BusId = bus.Id,
                                  Country = (from innerBus in context.Buses
                                             join innerWaypoint in context.WayPointsSchedules on innerBus.Id equals innerWaypoint.BusId
                                             join innerPath in context.Paths on innerWaypoint.PathId equals innerPath.Id
                                             join innerCity in context.Cities on innerPath.CityFromId equals innerCity.Id
                                             join innerCountry in context.Countries on innerCity.CountryId equals innerCountry.Id
                                             where innerWaypoint.Sequence == 1 && innerBus.Id == bus.Id
                                             select new 
                                             {
                                                 innerCountry.Name
                                             }).First().Name
                              }).Distinct().ToList();
                return Json(routes);
            }
        }

        //public JsonResult GetAllPaths()
        //{


        //    IEnumerable<IGrouping<dynamic, dynamic>> routes;

        //    if (cache.TryGetValue("Paths", out routes))
        //    {
        //        routes = ((IEnumerable<IGrouping<dynamic, dynamic>>)cache.Get("Paths")).Take(50);
        //        return Json(routes);
        //    }
        //    else
        //    {
        //        using (var context = new MapAppContext())
        //        {

        //            routes = (from bus in context.Buses
        //                      join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
        //                      join path in context.Paths on waypoint.PathId equals path.Id
        //                      join city in context.Cities on path.CityFromId equals city.Id
        //                      join country in context.Countries on city.CountryId equals country.Id
        //                      join coord in context.Coords on path.Id equals coord.PathId
        //                      orderby waypoint.Sequence
        //                      select new
        //                      {
        //                          BusId = bus.Id,
        //                          Sequence = waypoint.Sequence,
        //                          Country = country.Name,
        //                          City = city.Name,
        //                          PathId = waypoint.PathId,
        //                          Longtitude = coord.Longtitude,
        //                          Latitude = coord.Latitude
        //                      }).ToList().GroupBy(g => new { g.BusId });

        //            cache.Set("Paths", routes, new MemoryCacheEntryOptions
        //            {
        //                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(100)
        //            });
        //            return Json(routes);
        //        }
        //    }
        //}



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
            int citiesCount = 30;
            int searchRange = 4;

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

                for (int i = 0; i < 15; i++)
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
                    for (int j = 0; j < r.Next(3,8); j++)
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
                            || Math.Abs((city2.Latitude+city2.Longtitude)-(city1.Latitude+city1.Longtitude))> searchRange)
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
                                    (path.Id,
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
                            || Math.Abs((city2.Latitude + city2.Longtitude) - (city1.Latitude + city1.Longtitude)) > searchRange)
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
                                    (path.Id,
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
    
                        context.WayPointsSchedules.Add(new WayPointsSchedule()
                        {
                            BusId = instBus.Id,
                            Sequence = j + 1,
                            PathId = path.Id
                        });
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



            


            List<Coords> GetWayBetweenCities(string pathId, City cityFrom, City cityTo, int lastInsertedId, MapAppContext context)
            {
                List<Coords> coordinates = new List<Coords>();
                RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<string>() { cityFrom.Name, cityTo.Name });

                //string jsonString = JsonSerializer.Serialize<RoutingRequest>(routingRequest);
                //var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsJsonAsync(
                    "http://open.mapquestapi.com/directions/v2/route?key=iVOoDHSx5Ykdj4sIKnWbkmO2SgjbCOBI", routingRequest).Result;

                RoutingApiResponseModel responseModel = response.Content.ReadFromJsonAsync<RoutingApiResponseModel>().Result;


                for (int i = 0; i < responseModel._Route._Shape.ShapePoints.Length; i += 2)
                {
                    coordinates.Add(new Coords()
                    {
                        Id = lastInsertedId + i/2 + 1,
                        PathId = pathId,
                        Longtitude = responseModel._Route._Shape.ShapePoints[i],
                        Latitude = responseModel._Route._Shape.ShapePoints[i + 1]
                    });

                }
                coordinates = coordinates.OrderBy(c => c.Id).ToList();

                cityFrom.Longtitude = coordinates.First().Latitude;
                cityFrom.Latitude = coordinates.First().Longtitude;

                cityTo.Longtitude = coordinates.Last().Latitude;
                cityTo.Latitude = coordinates.Last().Longtitude;

                context.Cities.UpdateRange(cityFrom,cityTo);

                //string testjson = await response.Content.ReadAsStringAsync();

                // response.EnsureSuccessStatusCode();
                // var test = response.Headers.Location;

                return coordinates;

            }
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
