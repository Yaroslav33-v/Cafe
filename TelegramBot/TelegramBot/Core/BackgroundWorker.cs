using Npgsql;
using Dapper;
using Telegram.Bot;
using TelegramBot.Entities;

namespace TelegramBot.Core
{
    internal class BackgroundWorker
    {
        private static bool _isRunning = true;
        private static NpgsqlConnection _dbConnection = null!;
        private static ITelegramBotClient _botClient = null!;

        public BackgroundWorker(string connectionString, ITelegramBotClient botClient)
        {
            _dbConnection = new NpgsqlConnection(connectionString);
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
        }
        public async void DoBackgroundWork()
        {
            List<Offer> newOffers = [.. await _dbConnection.QueryAsync<Offer>
                ("SELECT * FROM public.offers" +
                "WHERE ")];
        }
    }
}
