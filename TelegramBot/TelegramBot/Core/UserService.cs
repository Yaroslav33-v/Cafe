using Dapper;
using Npgsql;
using Telegram.Bot.Types;

namespace TelegramBot.Core
{
    internal class UserService
    {
        private readonly string _connectionString;

        public UserService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RegisterUserInteraction(Message message)
        {
            const string sql = @"
                INSERT INTO public.telegram_chats (chat_id)
                VALUES (@ChatId)
                ON CONFLICT (chat_id) 
                DO UPDATE SET 
                    is_active = true";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new
                {
                    ChatId = message.Chat.Id,
                });
            }
        }
    }
}
