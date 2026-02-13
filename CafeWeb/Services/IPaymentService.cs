using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IPaymentService
    {
        Task<(bool, string?)> TryToPay(PaymentModel paymentModel);
    }
}
