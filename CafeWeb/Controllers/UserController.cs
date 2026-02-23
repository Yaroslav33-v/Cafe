using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    public class UserController : Controller
    {
        public IActionResult SignUp()
        {
            return View();
        }
    }
}
