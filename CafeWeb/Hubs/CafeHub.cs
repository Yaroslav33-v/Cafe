using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CafeWeb.Hubs
{
    public class CafeHub : Hub
    {
        // Потокобезопасный словарь для хранения связей userId -> ConnectionId
        private static readonly ConcurrentDictionary<int, string> _userConnections = new();

        public static string? GetConnectionId(int userId)
        {
            return _userConnections.TryGetValue(userId, out var connectionId)
                ? connectionId
                : null;
        }

        public static IReadOnlyDictionary<int, string> GetActiveConnections() => _userConnections;

        public async Task RegisterUser(int userId)
        {
            if (userId <= 0)
                throw new HubException("Invalid user ID");

            // Сохраняем связь userId -> ConnectionId
            _userConnections.AddOrUpdate(userId, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);

            // Добавляем в персональную группу пользователя
            await Groups.AddToGroupAsync(Context.ConnectionId, $"client-{userId}");

            // Уведомляем клиента об успешной регистрации
            await Clients.Caller.SendAsync("Registered", new
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                Message = "Successfully registered for real-time updates"
            });
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", new
            {
                ConnectionId = Context.ConnectionId,
                Message = "Connected to CafeHub"
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Находим и удаляем пользователя по ConnectionId
            var userEntry = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (userEntry.Key > 0)
                _userConnections.TryRemove(userEntry.Key, out _);

            if (exception != null)
                System.Diagnostics.Debug.WriteLine($"Пользователь {userEntry.Key} отключен. Ошибка: {exception.Message}");
            
            await base.OnDisconnectedAsync(exception);
        }

        // Метод для проверки статуса соединения
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.Now);
        }
    }
}
