using CafeWeb.Models;
using Stripe;

namespace CafeWeb.Services
{
    public class PaymentService : IPaymentService
    {
        Task GenerateTokenAsync(PaymentMethodService paymentMethodService,PaymentModel paymentModel)
        {
            return paymentMethodService.CreateAsync(new PaymentMethodCreateOptions
            {
                Card = new PaymentMethodCardOptions
                {
                    Number = paymentModel.CardNumber,
                    ExpMonth = int.Parse(paymentModel.MonthYear[0..2]),
                    ExpYear = int.Parse(paymentModel.MonthYear[^2..]),
                    Cvc = paymentModel.CvvCode
                }
            });
        }
        public Task<bool> TryToPay(PaymentModel paymentModel)
        {
            GenerateTokenAsync(new PaymentMethodService(), paymentModel);    
            throw new NotImplementedException();
        }
    }
}
