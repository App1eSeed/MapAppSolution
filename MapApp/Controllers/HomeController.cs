using MapApp.Models;
using MapApp.Models.EF;
using MapApp.Models.EF.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MapAppContext context;
        public HomeController(ILogger<HomeController> logger, MapAppContext mapAppContext)
        {
            _logger = logger;
            context = mapAppContext;
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
            var routes = context.Paths.ToList().GroupBy(p => p.BusId);

            return Json(routes);
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
