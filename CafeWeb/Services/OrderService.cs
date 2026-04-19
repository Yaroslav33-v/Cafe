using CafeWeb.Enums;
using CafeWeb.Models;
using CafeWeb.Static;
using Dapper;
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

        public async Task<int> CreateOrder(int userId, Order order, IDbTransaction transaction)
        {
            try
            {
                // 1. Вставляем заказ
                int orderId = await _connection.ExecuteScalarAsync<int>(@"
                    INSERT INTO public.orders 
                        (order_number, created_at, status, user_id)
                    VALUES 
                        (@OrderNumber, CURRENT_TIMESTAMP, @Status, @UserId)
                    RETURNING order_id", new
                    {
                        OrderNumber = order.Number,
                        order.Status,
                        UserId = userId
                    }, transaction);

                // 2. Вставляем позиции заказа
                var foodOrders = order.CartItems.Select(item => new
                    {
                        FoodId = item.Food.Id,
                        OrderId = orderId,
                        item.Quantity
                    });

                await _connection.ExecuteAsync(@"
                    INSERT INTO public.food_orders 
                        (food_id, order_id, food_quantity)
                    VALUES 
                        (@FoodId, @OrderId, @Quantity)", 
                    foodOrders, transaction);

                return orderId;
            }
            catch (Exception ex) 
            {
                _logger.LogError("Ошибка при создании заказа: {message}", ex.Message);
                throw new Exception("Ошибка при создании заказа");
            }
        }

        public async Task<Order> CreateOrderModel(Cart cart)
        {
            try
            {
                OrderNumberGenerator.InitializeFromLastOrderNumber(await GetLastOrderNumber());

                return new()
                {
                    Number = OrderNumberGenerator.GenerateOrderNumber(),
                    CartItems = cart.Items,
                    Status = OrderStatus.Pending
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при получении номера последнего заказа: {message}", ex.Message);
                throw new Exception("Ошибка при создании заказа. Оплата отменена");
            }
        }

        private async Task<string> GetLastOrderNumber() =>
            await _connection.QueryFirstOrDefaultAsync<string>(@"
                    SELECT order_number 
                    FROM public.orders  
                    ORDER BY order_id DESC 
                    LIMIT 1")
                ?? "AA000";

        public Task<List<Order>> GetOrders(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateOrderStatus(int id, string status, IDbTransaction? transaction = null)
        {
            int affected = await _connection.ExecuteAsync(@"
                UPDATE public.orders 
                SET status = @NewStatus
                WHERE order_id = @OrderId
                AND status != @NewStatus", 
                new { NewStatus = status, OrderId = id }, transaction);

            if (affected == 0)
                throw new Exception($"Не удалось обновить статус заказа {id}");
        }
    }
}
