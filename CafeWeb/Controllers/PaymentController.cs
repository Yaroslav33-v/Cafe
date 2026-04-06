using CafeWeb.Static;
using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    [Route("/payment/[action]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }
        
        public IActionResult Index(decimal total) 
        {
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
