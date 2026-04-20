using CafeWeb.Enums;
using CafeWeb.Models;
using CafeWeb.Services;
using CafeWeb.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CafeWeb.Controllers
{
    [Authorize]
    [Route("/payment/[action]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        public PaymentController(IPaymentService paymentService, ICartService cartService, IOrderService orderService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        public IActionResult Index() 
        {
            decimal total = _cartService.GetCart().TotalAmount;

            if (total <= 0)
                return Redirect("/cafe/cart");

            ViewBag.Total = total;
            ViewBag.Original = total;

            return View(); 
        }
        [RedirectOnly]
        public ViewResult PaymentFailed(string message) 
        {
            ViewBag.Message = message;
            return View();
        }
        [RedirectOnly]
        public ViewResult PaymentSucceeded() => View();
        [HttpPost]
        public async Task<IActionResult> Pay([FromForm] PaymentModel paymentModel)
        {
            try
            {
                var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (strId is not null && int.TryParse(strId, out int userId))
                {
                    // Создаем модель заказа
                    var order = await _orderService.CreateOrderModel(_cartService.GetCart());

                    // Вставляем заказ в БД
                    int orderId = await _orderService.CreateOrder(userId, order);

                    // Отправляем запрос на оплату
                    bool isSuccess = await _paymentService.TryToPay(paymentModel);

                    if (isSuccess)
                    {
                        // Изменяем статус заказа на "Готовится"
                        await _orderService.UpdateOrderStatus(orderId, OrderStatus.InProcess);
                            
                        // Очищаем корзину пользователя
                        _cartService.ClearCart();

                        // Устанавливаем маркер для атрибута
                        TempData["RedirectOnly"] = true;

                        return RedirectToAction("PaymentSucceeded");
                    }
                    else
                    {
                        // Изменяем статус заказа на "Ошибка оплаты"
                        await _orderService.UpdateOrderStatus(orderId, OrderStatus.Error);

                        throw new Exception("Ошибка при оплате");
                    }
                }

                throw new Exception("Не удалось получить информацию о пользователе. Оплата отменена");
            }
            catch(Exception ex)
            {
                TempData["RedirectOnly"] = true;
                return RedirectToAction("PaymentFailed", new { message = ex.Message });
            }  
        }
    }
}
