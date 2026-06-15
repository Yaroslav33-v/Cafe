using CafeWeb.Models;
using CafeWeb.Services;
using CafeWeb.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CafeWeb.Controllers
{
    [Route("/admin/[action]")]
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
        }
        public ViewResult Index() => View();
        public async Task<ViewResult> AddFood(string? message = null)
        {
            ViewBag.Message = message;
            var adminFoodModel = new AdminFoodModel
            {
                Categories = await _adminService.GetCategoryNames(),
                ExistingFood = await _adminService.GetAllFood()
            };

            return View(adminFoodModel);
        }
        public async Task<ViewResult> NewOffer(string? message = null)
        {
            ViewBag.Message = message;
            var adminOfferModel = new AdminOfferModel
            {
                Foods = await _adminService.GetAllFood()
            };
            return View(adminOfferModel);
        }
        public ViewResult NewPromo(string? message = null)
        {
            ViewBag.Message = message;
            return View();
        }
        public ViewResult NewAdmin(string? message = null)
        {
            ViewBag.Message = message;
            return View();
        }

        public async Task<ViewResult> History()
        {
            var orders = await _adminService.GetAllOrders();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> AddFood([FromForm] AdminFoodModel adminFoodModel)
        {
            try
            {
                await _adminService.InsertFood(adminFoodModel);
                return RedirectToAction("AddFood", new { message = "Позиция успешно добавлена!"});
            }
            catch (Exception ex) 
            {
                return RedirectToAction("AddFood", new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewOffer([FromForm] AdminOfferModel adminOfferModel)
        {
            try
            {
                await _adminService.InsertOffer(adminOfferModel);
                return RedirectToAction("NewOffer", new { message = "Акция успешно добавлена!" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("NewOffer", new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewPromo([FromForm] Promocode promocode)
        {
            try
            {
                await _adminService.InsertPromocode(promocode);
                return RedirectToAction("NewPromo", new { message = "Промокод успешно добавлен!" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("NewPromo", new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewAdmin([FromForm] User user)
        {
            try
            {
                await _adminService.InsertAdmin(user);
                return RedirectToAction("NewAdmin", new { message = "Админ успешно добавлен!" });
            }
            catch(Exception ex) 
            {
                return RedirectToAction("NewAdmin", new { message = ex.Message });
            }
        }
    }
}
