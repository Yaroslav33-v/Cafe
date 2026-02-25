using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IPaymentService
    {
        Task TryToPay(PaymentModel paymentModel);
    }
}
