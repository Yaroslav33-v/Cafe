using CafeWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    public class CafeController : Controller // Контроллер для кафе 
    {
        public IActionResult Index() 
        {
            // Метод для отображения представления (название метода должно совпадать с названием представления)
            ICollection<Food> pizzas = new List<Food>()
            {
                new Food{Name = "4 Творожка", Price = 1488, Description = "Та самая легендарная пиццы 4 Творожка"},
                new Food{Name = "Фа пепе шнейне", Price = 1337, Description = "Это же пицца фа для всех пепе от шнейне"}
            };
            return View(pizzas);
        }
    }
}
