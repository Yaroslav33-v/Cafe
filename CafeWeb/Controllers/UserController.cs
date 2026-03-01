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
        public IActionResult SignUp(string? referer = null, string? problem = null) 
        {
            ViewBag.Referer = referer ?? Request.Headers.Referer.ToString();
            ViewBag.Problem = problem;
            return View();
        }
        public IActionResult SignIn(string? problem = null)
        {
            ViewBag.Problem = problem;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp([FromForm]string? referer, [FromForm] User user)
        {
            try
            {
                await _userService.SignUp(user);

                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }
                return RedirectToAction("SignIn");
            }
            catch(Exception ex)
            {
                return RedirectToAction("SignIn", new { referer, problem = ex.Message });
            }
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
                return RedirectToAction("SignIn", new { problem = ex.Message });
            }
        }
    }
}
