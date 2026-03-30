using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafeWeb.Controllers
{
    public class CafeController : Controller // Контроллер для кафе 
    {
        private readonly ICafeService _cafeService;
        public CafeController(ICafeService cafeService)
        {
            _cafeService = cafeService ?? throw new ArgumentNullException(nameof(cafeService));
        }

        public async Task<IActionResult> Index() 
        {
            try
            {
                var categories = await _cafeService.GetFoods();
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId is not null)
                {
                    int id = int.Parse(userId); 
                    var favourite = await _cafeService.GetFavourites(id);
                    
                    if(favourite is not null && favourite.Foods.Any())
                        categories.Insert(0, favourite);
                }

                return View(categories);
            }
            catch
            {
                return Redirect("/error");
            }
        }
        public IActionResult Cart()
        {

            return View();
        }
        public IActionResult MyOrder()
        {
            return View();
        }
    }
}
