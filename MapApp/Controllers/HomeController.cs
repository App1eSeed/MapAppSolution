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
        //private MapAppContext context;
        private static readonly HttpClient client = new HttpClient();
        public HomeController(ILogger<HomeController> logger/*, MapAppContext mapAppContext*/)
        {
            _logger = logger;
           // context = mapAppContext;
        }

        public IActionResult Index()
        {

            //var paths = context.Set<Path>().ToList();
            //var routes = context.Set<Bus>().ToList();
            //var wayPointSchedules = context.Set<WayPointsSchedule>().ToList();
            //var cities = context.Set<City>().ToList();
            //var schedules = context.Set<Schedule>().ToList();
            //foreach (var route in routes)
            //{
            //    var path = route.Path;
            //    var wayPoints = route.WayPointsSchedule;
            //}
            //ViewBag.Routes = routes;
            //GenerateDB();
            return View();
            
        }

        
        public JsonResult GetAllPaths()
        {
            using (var context = new MapAppContext())
            {
                var routes = (from bus in context.Buses
                              join path in context.Paths on bus.Id equals path.BusId
                              //join city in context.Cities on bus.FromCityId equals city.Id
                              select new { BusId = bus.Id, city = "test",
                                  path.Longtitude, path.Latitude }).ToList().GroupBy(g => new { g.BusId, g.city });

                //select new { BusId = bus.Id, city = city.Name, path.Longtitude, path.Latitude }).ToList().GroupBy(g => new { g.BusId, g.city });


                return Json(routes);
            }
            //var routes = context.Paths.ToList().GroupBy(p => p.BusId);

            //(from innerWayPoint in context.WayPointsSchedules
            // join innerCity in context.Cities on innerWayPoint.CityId equals innerCity.Id
            // where innerWayPoint.BusId == bus.Id && innerWayPoint.Sequence == 1
            // select new { innerCity.Name }).FirstOrDefault().Name
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
                var route = context.Paths.Where(p => p.BusId == busId).ToList().OrderBy(p => p.Id);
                return Json(route);
            }
        }

        public void GenerateDB()
        {
            Random r = new Random();
            int n = 8;
            using (var context = new MapAppContext())
            {
                var Paths = new List<Path>();

                int lastInsertedId;
                if (context.Paths.OrderBy(p => p.Id).LastOrDefault() != null)
                {
                    lastInsertedId = context.Paths.OrderBy(p => p.Id).LastOrDefault().Id;
                }
                else
                {
                    lastInsertedId = 1;
                }

                for (int i = 0; i < 10; i++)
                {
                    Bus instBus = new Bus()
                    {
                        Operator = "Rand " + i,
                        //                    FromCityId = r.Next(1, 9).ToString()
                    };
                    context.Buses.Add(instBus);
                    var wayPointsList = new List<string>();
                    var test12 = new Dictionary<int, string>();
                    for (int j = 0; j < 4; j++)
                    {
                        int rand = r.Next(1, n);
                        while (context.Cities.Where(c => wayPointsList.Contains(c.Name)).Select(c => c.Id).ToList().Contains(rand.ToString()))
                        {
                            rand = r.Next(1, n);
                        }
                        context.WayPointsSchedules.Add(new WayPointsSchedule()
                        {
                            BusId = instBus.Id,
                            Sequence = j + 1,
                            CityId = rand.ToString()
                        });
                        wayPointsList.Add(context.Cities.Where(c => c.Id == rand.ToString()).FirstOrDefault().Name);
                        test12.Add(j + 1, context.Cities.Where(c => c.Id == rand.ToString()).FirstOrDefault().Name);
                    }

                    //var query = (from city in context.Cities
                    //            join wayPoint in context.WayPointsSchedules on city.Id equals wayPoint.CityId
                    //            where wayPoint.BusId == instBus.Id 
                    //            select new {wayPoint.Sequence,city.Name}).OrderBy(wp => wp.Sequence).ToList();
                    //var wayPoints = new List<string>();
                    //foreach (var wayPoint in query)
                    //{
                    //    wayPoints.Add(wayPoint.Name);
                    //}
                    
                    

                    Paths.AddRange(GetWayBetweenCities(instBus.Id, wayPointsList, lastInsertedId));
                    lastInsertedId = lastInsertedId + Paths.Count;
                }

                context.Paths.AddRange(Paths.OrderBy(p => p.BusId));
                context.Database.OpenConnection();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Paths ON;");
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Paths OFF;");
                }
                finally
                {
                    context.Database.CloseConnection();
                }
                

            }





            List<Path> GetWayBetweenCities(string busId, List<string> cities, int lastInsertedId)
            {
                List<Path> coordinates = new List<Path>();
                RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(cities);

                //string jsonString = JsonSerializer.Serialize<RoutingRequest>(routingRequest);
                //var httpContent = new StringContent(jsonString, Encoding.UTF8");, "application/json

                HttpResponseMessage response = client.PostAsJsonAsync(
                    "http://open.mapquestapi.com/directions/v2/route?key=iVOoDHSx5Ykdj4sIKnWbkmO2SgjbCOBI", routingRequest).Result;

                RoutingApiResponseModel responseModel = response.Content.ReadFromJsonAsync<RoutingApiResponseModel>().Result;


                for (int i = 0; i < responseModel._Route._Shape.ShapePoints.Length; i += 2)
                {
                    coordinates.Add(new Path()
                    {
                        Id = lastInsertedId + i/2 + 1,
                        BusId = busId,
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
