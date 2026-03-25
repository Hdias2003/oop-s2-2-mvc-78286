using Microsoft.AspNetCore.Mvc;

namespace oop_s2_2_mvc_78286.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
