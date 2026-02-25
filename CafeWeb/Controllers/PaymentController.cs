using CafeWeb.Models;
using CafeWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
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
            return View(); 
        }
        public ViewResult PaymentFailed(string message) 
        {
            ViewBag.Message = message;
            return View();
        }
        public ViewResult PaymentSucceeded() => View();
        [HttpPost]
        public async Task<IActionResult> Pay([FromForm] PaymentModel paymentModel)
        {

            try
            {
                await _paymentService.TryToPay(paymentModel);
                return RedirectToAction("PaymentSucceeded");
            }
            catch(Exception ex)
            {
                return RedirectToAction("PaymentFailed", new { message = ex.Message });
            }  
        }
    }
}
