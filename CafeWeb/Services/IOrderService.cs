using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderModel(Cart cart); 
        Task UpdateOrderStatus(int id, string status);
        Task<int> CreateOrder(int userId, Order order);
        Task<List<Order>> GetOrders(int userId);
    }
}
