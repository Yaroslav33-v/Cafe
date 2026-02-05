using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
