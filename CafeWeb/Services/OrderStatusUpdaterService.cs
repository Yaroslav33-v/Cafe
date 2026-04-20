using CafeWeb.Hubs;
using CafeWeb.Models;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using System.Data;

namespace CafeWeb.Services
{
    public class OrderStatusUpdaterService : BackgroundService
    {
        private readonly ILogger<OrderStatusUpdaterService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(1); // Проверка каждую минуту
        private readonly int _cookingDurationSeconds = 5; // Время приготовления

        public OrderStatusUpdaterService(ILogger<OrderStatusUpdaterService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис обновления статусов заказов запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateOrdersStatusAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ошибка при обновлении статуса заказов: {message}", ex.Message);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task UpdateOrdersStatusAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            // Получаем зависимости из DI
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<CafeHub>>();
            var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            // Находим заказы со статусом "Готовится", созданные более 10 секунд назад
            var cutoffTime = DateTime.Now.AddSeconds(-_cookingDurationSeconds);

            var ordersToUpdate = await dbConnection.QueryAsync<Order>(
                @"SELECT 
                    order_id as Id, 
                    order_number as Number, 
                    created_at as CreatedAt, 
                    status
                  FROM orders 
                  WHERE status = 'Готовится' 
                    AND created_at <= @CutoffTime
                    AND done_at IS NULL",
                new { CutoffTime = cutoffTime });

            foreach (var order in ordersToUpdate)
            {
                _logger.LogInformation(
                    "Заказ {OrderNumber} (ID: {OrderId}) готов. Изменяем статус на 'Готов'",
                    order.Number, order.Id);

                // Обновляем статус в БД
                await dbConnection.ExecuteAsync(
                    @"UPDATE orders 
                  SET status = 'Готов', 
                      done_at = @DoneAt 
                  WHERE order_id = @OrderId",
                    new
                    {
                        OrderId = order.Id,
                        DoneAt = DateTime.Now
                    });

                // Получаем client_id для уведомления
                int? userId = await dbConnection.QuerySingleOrDefaultAsync<int>(
                    "SELECT user_id FROM orders WHERE order_id = @OrderId",
                    new { OrderId = order.Id });

                // Отправляем уведомление клиенту через SignalR
                if (userId is int id)
                {
                    var updateInfo = new
                    {
                        OrderId = order.Id,
                        OrderNumber = order.Number,
                        Status = "Готов",
                        Message = $"✅ Заказ #{order.Number} готов! Можно забирать.",
                        Timestamp = DateTime.Now
                    };

                    await hubContext.Clients.Group($"client-{id}")
                    .SendAsync("OrderUpdated", updateInfo);

                    var connectionId = CafeHub.GetConnectionId(id);
                    if (!string.IsNullOrEmpty(connectionId))
                        await hubContext.Clients.Client(connectionId)
                            .SendAsync("OrderUpdated", updateInfo);
                }
            }

            if (ordersToUpdate.Any())
                _logger.LogInformation("Обновлено статусов заказов: {count} ", ordersToUpdate.Count());
        }
    }
}
