
using System.Data;

namespace PaymentApi.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly IDbConnection _connection;
        public BalanceService(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }
        public Task<decimal> GetBalanceByLastFour(string lastFour)
        {
            throw new NotImplementedException();
        }
    }
}
