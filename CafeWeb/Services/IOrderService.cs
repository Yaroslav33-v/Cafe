using CafeWeb.Enums;
using CafeWeb.Models;
using System.Data;

namespace CafeWeb.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderModel(Cart cart); 
        Task UpdateOrderStatus(int id, string status, IDbTransaction? transaction = null);
        Task<int> CreateOrder(int userId, Order order, IDbTransaction transaction);
        Task<List<Order>> GetOrders(int userId);
    }
}
