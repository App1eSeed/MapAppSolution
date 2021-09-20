using MapApp.Models;
using MapApp.Models.ApiRequestModels;
using MapApp.Models.EF;
using MapApp.Models.EF.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {         
            //GenerateDB();
            return View();     
        }


        public JsonResult GetAllPaths()
        {
            using (var context = new MapAppContext())
            {
                var routes = (from bus in context.Buses 
                join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                join path in context.Paths on waypoint.PathId equals path.Id
                join city in context.Cities on path.CityFromId equals city.Id
                join coord in context.Coords on path.Id equals coord.PathId
                select new
                {
                    BusId = bus.Id,
                    Sequence = waypoint.Sequence,
                    city = city.Name,
                    coord.Longtitude,
                    coord.Latitude
                }).ToList().OrderBy(r => r.Sequence).GroupBy(g => new { g.BusId });


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
        public JsonResult GetWay(string busId)
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
            int n = 19;
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

                for (int i = 0; i < 15; i++)
                {
                    Bus instBus = new Bus()
                    {
                        Operator = "Rand " + i,
                    };
                    context.Buses.Add(instBus);

                    int rand1 = r.Next(1, n);
                    int rand2 = r.Next(1, n);
                    City city1 = context.Cities.Where(c => c.Id == rand1.ToString()).FirstOrDefault();
                    City city2 = context.Cities.Where(c => c.Id == rand2.ToString()).FirstOrDefault();
                    for (int j = 0; j < r.Next(2,6); j++)
                    {
                        Path path = new Path();
                        List<Path> paths = new List<Path>();

                        if (j % 2 == 0)
                        {
                            rand2 = r.Next(1, n);
                            city2 = context.Cities.Where(c => c.Id == rand2.ToString()).FirstOrDefault();
                            while (paths.Where(p => p.CityToId == rand2.ToString() 
                                || p.CityFromId == rand2.ToString()).FirstOrDefault() != null 
                            || rand1 == rand2
                            || Math.Abs((city2.Latitude+city2.Longtitude)-(city1.Latitude+city1.Longtitude))> 7)
                            {
                                rand2 = r.Next(1, n);
                                city2 = context.Cities.Where(c => c.Id == rand2.ToString()).FirstOrDefault();
                            }

                            var check = context.Paths.Where(p => p.CityFromId == rand1.ToString() && p.CityToId == rand2.ToString()).FirstOrDefault();

                            if (check != null)
                            {
                                paths.Add(check);
                                path = check;
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
                                    context.Cities.Where(c => c.Id == path.CityFromId).FirstOrDefault().Name,
                                    context.Cities.Where(c => c.Id == path.CityToId).FirstOrDefault().Name,
                                    lastInsertedId
                                    ));
                                
                                lastInsertedId = lastInsertedId + coords.Count;
                                paths.Add(path);
                            }

                        }
                        else
                        {
                            rand1 = r.Next(1, n);
                            city1 = context.Cities.Where(c => c.Id == rand1.ToString()).FirstOrDefault();
                            while (paths.Where(p => p.CityFromId == rand1.ToString()
                               || p.CityToId == rand1.ToString()).FirstOrDefault() != null
                            || rand1 == rand2
                            || Math.Abs((city2.Latitude + city2.Longtitude) - (city1.Latitude + city1.Longtitude)) > 7)
                            {
                                rand1 = r.Next(1, n);
                                city1 = context.Cities.Where(c => c.Id == rand1.ToString()).FirstOrDefault();
                            }

                            var check = context.Paths.Where(p => p.CityFromId == rand2.ToString() && p.CityToId == rand1.ToString()).FirstOrDefault();

                            if (check != null)
                            {
                                paths.Add(check);
                                path = check;
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
                                    context.Cities.Where(c => c.Id == path.CityFromId).FirstOrDefault().Name,
                                    context.Cities.Where(c => c.Id == path.CityToId).FirstOrDefault().Name, 
                                    lastInsertedId
                                    ));
                                lastInsertedId = lastInsertedId + coords.Count;
                                paths.Add(path);
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



            


            List<Coords> GetWayBetweenCities(string pathId, string cityFrom, string cityTo, int lastInsertedId)
            {
                List<Coords> coordinates = new List<Coords>();
                RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<string>() { cityFrom, cityTo });

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
