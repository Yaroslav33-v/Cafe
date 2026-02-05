using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    public class CafeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
