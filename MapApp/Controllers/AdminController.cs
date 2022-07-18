using Microsoft.AspNetCore.Mvc;

namespace MapApp.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
