using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MapApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {


            return View();
        }
        [Authorize]
        public IActionResult RoutesOffers()
        {
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
