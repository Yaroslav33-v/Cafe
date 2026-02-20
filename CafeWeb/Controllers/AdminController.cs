using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CafeWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
        }
        public async Task<ViewResult> AddFood()
        {
            var adminFoodModel = new AdminFoodModel
            {
                Categories = await _adminService.GetCategoryNames()
            };

            return View(adminFoodModel);
        }
        public ViewResult NewOffer() => View();
        public ViewResult NewPromo(string? errorMsg = null)
        {
            ViewBag.Problem = errorMsg;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddFood([FromForm] AdminFoodModel adminFoodModel)
        {
            await _adminService.InsertFood(adminFoodModel);
            return RedirectToAction("Index", "Cafe");
        }

        [HttpPost]
        public async Task<IActionResult> NewOffer([FromForm] Offer offer)
        {
            await _adminService.InsertOffer(offer);
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
