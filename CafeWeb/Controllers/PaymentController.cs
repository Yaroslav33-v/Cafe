using CafeWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace CafeWeb.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index() 
        {
            ViewBag.Total = 100.00;
            return View(); 
        }
        public IActionResult PaymentFailed(PaymentModel paymentModel) => View(paymentModel);
        public IActionResult PaymentSucceeded() => View();
        [HttpPost]
        public IActionResult Pay([FromForm] PaymentModel paymentModel)
        {
            return RedirectToAction("PaymentFailed", paymentModel);
        }
    }
}
