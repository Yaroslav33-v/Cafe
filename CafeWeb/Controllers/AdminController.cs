using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ViewResult> AddFood(string? errorMsg = null)
        {
            ViewBag.Problem = errorMsg;
            var adminFoodModel = new AdminFoodModel
            {
                Categories = await _adminService.GetCategoryNames()
            };

            return View(adminFoodModel);
        }
        public async Task<ViewResult> NewOffer(string? errorMsg = null)
        {
            ViewBag.Problem = errorMsg;
            var adminOfferModel = new AdminOfferModel
            {
                Foods = await _adminService.GetAllFood()
            };
            return View(adminOfferModel);
        }
        public ViewResult NewPromo(string? errorMsg = null)
        {
            ViewBag.Problem = errorMsg;
            return View();
        }

        public ViewResult NewAdmin(string? errorMsg = null)
        {
            ViewBag.Problem = errorMsg;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddFood([FromForm] AdminFoodModel adminFoodModel)
        {
            try
            {
                await _adminService.InsertFood(adminFoodModel);
                return RedirectToAction("Index", "Cafe");
            }
            catch (Exception ex) 
            {
                return RedirectToAction("AddFood", new { errorMsg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewOffer([FromForm] AdminOfferModel adminOfferModel)
        {
            try
            {
                await _adminService.InsertOffer(adminOfferModel);
                return RedirectToAction("Index", "Cafe");
            }
            catch (Exception ex)
            {
                return RedirectToAction("NewOffer", new { errorMsg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewPromo([FromForm] Promocode promocode)
        {
            try
            {
                await _adminService.InsertPromocode(promocode);
                return RedirectToAction("Index", "Cafe");
            }
            catch (Exception ex)
            {
                return RedirectToAction("NewPromo", new { errorMsg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewAdmin([FromForm] User user)
        {
            try
            {
                await _adminService.InsertAdmin(user);
                return RedirectToAction("Index", "Cafe");
            }
            catch(Exception ex) 
            {
                return RedirectToAction("NewAdmin", new { errorMsg = ex.Message });
            }
        }
    }
}
