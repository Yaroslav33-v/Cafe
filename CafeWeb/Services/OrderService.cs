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

        public async Task<int> CreateOrder(int userId, Order order)
        {
            // Открываем соединение вручную для транзакций
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            using var transaction = _connection.BeginTransaction();
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

                transaction.Commit();
                return orderId;
            }
            catch (Exception ex) 
            {
                transaction.Rollback();
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

        public async Task<List<Order>> GetOrders(int userId, bool all = false)
        {
            string sql = @"SELECT 
                    o.order_id AS Id,
                    o.order_number AS Number,
                    o.created_at AS CreatedAt,
                    o.status AS Status,
                    o.done_at AS DoneAt,
                    f.food_id AS Id,
                    f.food_name AS Name,
                    f.price AS Price,
                    f.calories AS Calories,
                    f.weight AS Weight,
                    f.ingredients AS Ingredients,
                    f.description AS Description,
                    f.front_image_address AS FrontImageAddress,
                    f.back_image_address AS BackImageAddress,
                    f.category_id AS CategoryId,
                    fo.food_quantity AS Quantity
                FROM public.orders o
                LEFT JOIN public.food_orders fo ON o.order_id = fo.order_id
                LEFT JOIN public.food f ON fo.food_id = f.food_id
                WHERE o.user_id = @UserId
                 AND o.status IN (@InProcess, @Ready)";

            if (!all)
                sql += @"
                  AND o.created_at >= NOW() - INTERVAL '1 hour'
                ORDER BY o.created_at DESC, o.order_id, f.food_id";
            else
                sql += @"
                ORDER BY o.created_at DESC, o.order_id, f.food_id";

            var orderDictionary = new Dictionary<int, Order>();

            await _connection.QueryAsync<Order, Food, CartItem, Order>(sql,
                (order, food, cartItem) =>
                {
                    // Проверяем, есть ли уже такой заказ в словаре
                    if (!orderDictionary.TryGetValue(order.Id, out var currentOrder))
                    {
                        // Если нет - создаем новый
                        currentOrder = order;
                        currentOrder.CartItems = new List<CartItem>();
                        orderDictionary.Add(currentOrder.Id, currentOrder);
                    }

                    // Если есть еда и количество > 0 - добавляем в корзину заказа
                    if (food != null && cartItem != null && cartItem.Quantity > 0)
                    {
                        currentOrder.CartItems.Add(new CartItem
                        {
                            Food = food,
                            Quantity = cartItem.Quantity
                        });
                    }

                    return currentOrder;
                },
                new { UserId = userId, OrderStatus.InProcess, OrderStatus.Ready},
                splitOn: "Id, Quantity"  // Указываем, где заканчивается Order и начинается Food
            );

            return orderDictionary.Values.ToList();
        }

        public async Task UpdateOrderStatus(int id, string status)
        {
            int affected = await _connection.ExecuteAsync(@"
                UPDATE public.orders 
                SET status = @NewStatus
                WHERE order_id = @OrderId
                AND status != @NewStatus", 
                new { NewStatus = status, OrderId = id });

            if (affected == 0)
                throw new Exception($"Не удалось обновить статус заказа");
        }
    }
}
