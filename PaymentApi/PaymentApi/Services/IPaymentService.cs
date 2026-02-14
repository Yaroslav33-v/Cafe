using PaymentApi.Dto;

namespace PaymentApi.Services
{
    public interface IPaymentService
    {
        Task TryToPay(PaymentDto paymentDto);
    }
}
