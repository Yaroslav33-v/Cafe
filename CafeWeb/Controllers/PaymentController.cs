using CafeWeb.Static;
using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CafeWeb.Controllers
{
    [Authorize]
    [Route("/payment/[action]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ICartService _cartService;
        public PaymentController(IPaymentService paymentService, ICartService cartService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
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
                await _paymentService.TryToPay(paymentModel);
                // Устанавливаем маркер для атрибута
                TempData["RedirectOnly"] = true;

                return RedirectToAction("PaymentSucceeded");
            }
            catch(Exception ex)
            {
                TempData["RedirectOnly"] = true;
                return RedirectToAction("PaymentFailed", new { message = ex.Message });
            }  
        }
    }
}
