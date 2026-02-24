using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
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

        //public ViewResult NewAdmin(

        [HttpPost]
        public async Task<IActionResult> AddFood([FromForm] AdminFoodModel adminFoodModel)
        {
            (bool isAdded, string? msg) = await _adminService.InsertFood(adminFoodModel);
            if(isAdded) 
                return RedirectToAction("Index", "Cafe");

            return RedirectToAction("AddFood", new { errorMsg = msg });
        }

        [HttpPost]
        public async Task<IActionResult> NewOffer([FromForm] AdminOfferModel adminOfferModel)
        {
            (bool isAdded, string? msg) = await _adminService.InsertOffer(adminOfferModel);
            if(isAdded)
                return RedirectToAction("Index", "Cafe");

            return RedirectToAction("NewOffer", new { errorMsg = msg });
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
