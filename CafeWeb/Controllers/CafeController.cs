using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
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
            // Метод для отображения представления (название метода должно совпадать с названием представления)
            var categories = await _cafeService.GetFoods(); 
            return View(categories);
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
