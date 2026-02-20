using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
        }
        public async Task<ViewResult> AddFood(string? errorMsg = null)
        {
            ViewBag.Problem = errorMsg;
            var adminFoodModel = new AdminFoodModel
            {
                Categories = await _adminService.GetCategoryNames()
            };

            return View(adminFoodModel);
        }
        public async Task<ViewResult> NewOffer()
        {
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

        [HttpPost]
        public async Task<IActionResult> AddFood([FromForm] AdminFoodModel adminFoodModel)
        {
            (bool isAdded, string? msg) = await _adminService.InsertFood(adminFoodModel);
            if(isAdded) 
                return RedirectToAction("Index", "Cafe");

            return RedirectToAction();
        }

        [HttpPost]
        public async Task<IActionResult> NewOffer([FromForm] AdminOfferModel adminOfferModel)
        {
            await _adminService.InsertOffer(adminOfferModel);
            return RedirectToAction("Index", "Cafe");
        }

        [HttpPost]
        public async Task<IActionResult> NewPromo([FromForm] Promocode promocode)
        {
            (bool isAdded, string? msg) = await _adminService.InsertPromocode(promocode);
            if (isAdded) 
                return RedirectToAction("Index", "Cafe");

            return RedirectToAction("NewPromo", new { errorMsg = msg });
        }
    }
}
