using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CafeWeb.Controllers
{
    public class CafeController : Controller // Контроллер для кафе 
    {
        private readonly ICafeService _cafeService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        public CafeController(ICafeService cafeService, ICartService cartService, IOrderService orderService)
        {
            _cafeService = cafeService ?? throw new ArgumentNullException(nameof(cafeService));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
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
        public async Task<IActionResult> AddToCart(int foodId)
        {
            if (foodId <= 0)
                return BadRequest(new 
                { 
                    success = false,
                    message = "Неверный ID блюда" 
                });

            try
            {
                // Получаем блюдо
                Food food = await _cafeService.GetFood(foodId);

                // Добавляем блюдо в сессию
                _cartService.AddToCart(food);

                return Ok(new
                {
                    success = true,
                    message = $"Блюдо '{food.Name}' добавлено в корзину!"
                });
            }
            catch
            {
                return StatusCode(500, 
                    new
                    {
                        success = false,
                        message = "Серверная ошибка. Не удалось добавить блюдо в корзину"
                    });
            }
        }
        public IActionResult Cart() => View(_cartService.GetCart());
        public IActionResult ClearCart()
        {
            try
            {
                _cartService.ClearCart();

                return Json(new
                {
                    success = true,
                    message = "Корзина очищена",
                    cartCount = 0,
                    totalAmount = 0
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "Не удалось очистить корзину"
                });
            }
        }
        
        public IActionResult UpdateQuantity(int id, int value)
        {
            try
            {
                // Валидация
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Неверный ID блюда" });

                var cart = _cartService.GetCart();
                var cartItem = cart.Items.FirstOrDefault(i => i.Food.Id == id);

                if (cartItem == null)
                    return BadRequest(new { success = false, message = "Ошибка при получении блюда" });

                // Обновляем количество
                _cartService.UpdateQuantity(id, cartItem.Quantity + value);

                // Получаем обновленную корзину
                var updatedCart = _cartService.GetCart();
                var updatedCartItem = updatedCart.Items.FirstOrDefault(i => i.Food.Id == id);

                // Возвращаем обновленные данные
                return Ok(new
                {
                    success = true,
                    itemTotal = updatedCartItem?.Total ?? 0,
                    totalAmount = updatedCart.TotalAmount,
                    totalItems = updatedCart.TotalItems,
                    itemQuantity = updatedCartItem?.Quantity ?? 0,
                    itemId = id
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при обновлении количества"
                });
            }
        }

        public async Task<IActionResult> MyOrder(int orderId)
        {
            try
            {
                var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if(strId is not null && int.TryParse(strId, out int id))
                {
                    Order? order = await _orderService.GetOrderById(id, orderId);

                    return View(order);
                }

                return Redirect("/signout");
            }
            catch
            {
                return Redirect("/cafe/index");
            }
        }

        public async Task<IActionResult> UpdateFavourite(int foodId)
        {
            try
            {
                var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (strId is null || !int.TryParse(strId, out _))
                    throw new KeyNotFoundException();

                await _cafeService.UpdateFavourite(foodId, int.Parse(strId));

                return Ok(new
                {
                    success = true,
                    message = "Содержимое избранного успешно изменено"
                });
            }
            catch (KeyNotFoundException)
            {
                return Redirect("/signout");
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Не удалось изменить избранное"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOfferToCart([FromBody] CartOfferModel model)
        {
            try
            {
                var foods = await _cafeService.GetFoodsByIds(model.FoodIds);

                if (foods is null || !foods.Any())
                    return BadRequest(new
                    {
                        success = false,
                        message = "Не удалось найти блюда"
                    });

                _cartService.AddOfferToCart(new OfferCartItem
                {
                    Id = model.Id,
                    Name = model.Name,
                    Discount = model.Discount,
                    Foods = foods
                });

                return Ok(new
                {
                    success = true,
                    message = "Акция успешно добавлена в корзину"
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при добавлении акции в корзину. Попробуйте позже"
                });
            }
        }

        public IActionResult UpdateOfferQuantity(int offerId, int value)
        {
            try
            {
                // Валидация
                if (offerId <= 0)
                    return BadRequest(new { success = false, message = "Неверный ID блюда" });

                var cart = _cartService.GetCart();
                var cartItem = cart.OfferItems.FirstOrDefault(i => i.Id == offerId);

                if (cartItem == null)
                    return BadRequest(new { success = false, message = "Ошибка при получении блюда" });

                // Обновляем количество
                _cartService.UpdateOfferQuantity(offerId, cartItem.Quantity + value);

                // Получаем обновленную корзину
                var updatedCart = _cartService.GetCart();
                var updatedCartItem = updatedCart.OfferItems.FirstOrDefault(i => i.Id == offerId);

                // Возвращаем обновленные данные
                return Ok(new
                {
                    success = true,
                    itemTotal = updatedCartItem?.Total ?? 0,
                    totalAmount = updatedCart.TotalAmount,
                    totalItems = updatedCart.TotalItems,
                    itemQuantity = updatedCartItem?.Quantity ?? 0,
                    itemId = offerId
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при обновлении количества"
                });
            }
        }
    }
}
