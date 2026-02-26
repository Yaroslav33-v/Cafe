using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CafeWeb.Controllers
{
    [Route("/user/[action]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        public IActionResult SignUp() => View();
        public IActionResult SignIn() => View();

        [HttpPost]
        public async Task<IActionResult> SignUp([FromForm] User user)
        {
            await _userService.SignUp(user);
            return RedirectToAction("Index", "Cafe");
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] string? returnUrl, [FromForm] User user)
        {
            try
            {
                ClaimsIdentity claimsIdentity = await _userService.SignIn(user);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return Redirect(returnUrl ?? "/Cafe/Index");
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return Redirect("/access-denied");
            }
        }
    }
}
