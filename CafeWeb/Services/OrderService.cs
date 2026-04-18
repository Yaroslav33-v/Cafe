using System.Data;

namespace CafeWeb.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IDbConnection connection, ILogger<OrderService> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
