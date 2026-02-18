using Npgsql;
using Dapper;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Core
{
    public class NotificationService
    {
        private readonly string _connectionString;
        private readonly ITelegramBotClient _botClient;

        public NotificationService(string connectionString, ITelegramBotClient botClient)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
        }

        public async Task SendNotificationToAllUsers(string message, CancellationToken ct)
        {
            List<long> chatIds;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                chatIds = (await connection.QueryAsync<long>(
                    @"SELECT chat_id 
                  FROM public.telegram_chats 
                  WHERE is_active = true"
                )).ToList();
            }

            Console.WriteLine($"Начинаем рассылку {chatIds.Count} пользователям");

            int successCount = 0;
            int failCount = 0;

            foreach (var chatId in chatIds)
            {
                if (ct.IsCancellationRequested)
                    break;

                try
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: message,
                        parseMode: ParseMode.Html,
                        cancellationToken: ct
                    );

                    successCount++;

                    // Задержка чтобы не превысить лимиты Telegram (30 сообщений/секунду)
                    await Task.Delay(35, ct);
                }
                catch (Exception ex)
                {
                    failCount++;
                    Console.WriteLine($"Ошибка отправки пользователю {chatId}: {ex.Message}");

                    // Если пользователь заблокировал бота - помечаем как неактивного
                    if (ex.Message.Contains("Forbidden") || ex.Message.Contains("blocked"))
                    {
                        await MarkUserInactive(chatId);
                    }
                }
            }

            Console.WriteLine($"Рассылка завершена. Успешно: {successCount}, Ошибок: {failCount}");
            AppLogger.LogInfo($"Рассылка завершена. Успешно: {successCount}, Ошибок: {failCount}");
        }

        private async Task MarkUserInactive(long chatId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE public.bot_users SET is_active = false WHERE chat_id = @ChatId",
                new { ChatId = chatId }
            );
        }
    }
}
