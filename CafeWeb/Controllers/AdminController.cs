using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult AddFood()
        {
            return View();
        }
    }
}
