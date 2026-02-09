namespace PaymentApi.Services
{
    public interface IBalanceService
    {
        Task<decimal> GetBalanceByLastFour(string lastFour);
    }
}
