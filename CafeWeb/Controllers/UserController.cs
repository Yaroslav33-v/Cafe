using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CafeWeb.Controllers
{
    [Route("/user/[action]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ICafeService _cafeService;
        public UserController(IUserService userService, IOrderService orderService, ICafeService cafeService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _cafeService = cafeService ?? throw new ArgumentNullException(nameof(cafeService));
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
                    return Redirect(referer);

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

        [Authorize]
        public IActionResult Me()
        {
            var name = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            var role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (name is null || role is null || strId is null || !int.TryParse(strId, out _))
                return Redirect("/signout");

            ViewBag.Login = name;
            ViewBag.Role = role;
            ViewBag.Id = int.Parse(strId);

            return View();
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory(int userId)
        {
            try
            {
                if(userId == 0)
                {
                    var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (strId is null)
                        return Redirect("/signout");

                    userId = int.Parse(strId);
                }

                List<Order> orders = await _orderService.GetOrders(userId, all: true);
                return View(orders);
            }
            catch
            {
                return Redirect("/user/me");
            }
        }

        [Authorize]
        public async Task<IActionResult> Favourite(int userId)
        {
            try
            {
                if (userId == 0)
                {
                    var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (strId is null)
                        return Redirect("/signout");

                    userId = int.Parse(strId);
                }

                Category? fav = await _cafeService.GetFavourites(userId);
                return View(fav);
            }
            catch
            {
                return Redirect("/user/me");
            }
        }

        [Authorize]
        public async Task<IActionResult> Offers()
        {
            try
            {
                List<OfferUserModel> offers = await _userService.GetOffers();
                return View(offers);
            }
            catch
            {
                return Redirect("/user/me");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromForm] int userId,
            [FromForm] string currentPassword,
            [FromForm] string newPassword)
        {
            try
            {
                if (userId == 0)
                {
                    var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (strId is null)
                        return Redirect("/signout");

                    userId = int.Parse(strId);
                }

                bool success = await _userService.ChangePassword(userId, currentPassword, newPassword);

                if (success)
                    return Ok(new
                    {
                        success,
                        message = "Пароль успешно обновлен"
                    });

                return Ok(new
                {
                    success,
                    message = "Введен неправильный текущий пароль"
                });
            }
            catch (KeyNotFoundException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Не удалось получить данные пользователя"
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Серверная ошибка"
                });
            }
        }
    }
}
