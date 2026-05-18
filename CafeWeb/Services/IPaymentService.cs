using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IPaymentService
    {
        Task<bool> TryToPay(PaymentModel paymentModel);
    }
}
