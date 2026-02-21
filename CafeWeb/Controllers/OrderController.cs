using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MyOrder()
        {
            return View();
        }
        public IActionResult History()
        {
            return View();
        }
    }
}
