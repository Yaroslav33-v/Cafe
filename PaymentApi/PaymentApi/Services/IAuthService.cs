namespace PaymentApi.Services
{
    public interface IAuthService
    {
        Task<string> Login(string cardToken);
    }
}
