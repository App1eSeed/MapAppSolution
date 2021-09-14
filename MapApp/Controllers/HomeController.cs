using MapApp.Models;
using MapApp.Models.EF;
using MapApp.Models.EF.Entities;
using Microsoft.AspNetCore.Mvc;
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

            return View();
            
        }

        
        public JsonResult GetAllPaths()
        {
            using (var context = new MapAppContext())
            {
                var routes = (from bus in context.Buses
                              join path in context.Paths on bus.Id equals path.BusId
                              join city in context.Cities on bus.FromCityId equals city.Id
                              select new { BusId = bus.Id, city = city.Name, path.Longtitude, path.Latitude }).ToList().GroupBy(g => new { g.BusId, g.city });

                return Json(routes);
            }
            //var routes = context.Paths.ToList().GroupBy(p => p.BusId);

            
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
                var route = context.Paths.Where(p => p.BusId == busId).ToList();
                return Json(route);
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
