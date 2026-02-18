using Dapper;
using Npgsql;
using System.Text;
using Telegram.Bot;
using TelegramBot.Entities;

namespace TelegramBot.Core
{
    internal class BackgroundWorker
    {
        private readonly string _connectionString = null!;
        private readonly NotificationService _notificationService;
        private readonly CancellationToken _ct;

        public BackgroundWorker(string connectionString, ITelegramBotClient botClient, CancellationToken ct)
        {
            _connectionString = connectionString;
            _notificationService = new(_connectionString, botClient);
            _ct = ct;
        }
        public async Task<List<Offer>> GetNewOffers()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(_ct);
            using var transaction = connection.BeginTransaction();
            IEnumerable<Offer> newOffers = await connection.QueryAsync<Offer>(
                @"SELECT 
                    offer_id AS Id,
                    offer_name AS Name,
                    description AS Description,
                    discount AS Discount,
                    starts_at AS StartsAt,
                    ends_at AS EndsAt,
                    is_notificated AS IsNotificated
                FROM public.offers 
                WHERE CURRENT_DATE >= starts_at 
                AND is_notificated = false
                FOR UPDATE",  // Блокируем строки
                transaction: transaction
            );

            if (newOffers.Any())
            {
                var ids = newOffers.Select(o => o.Id);
                await connection.ExecuteAsync(@"
                    UPDATE public.offers 
                    SET is_notificated = true 
                    WHERE offer_id = ANY(@Ids)",
                    new { Ids = ids.ToArray() },
                    transaction: transaction
                );
            }

            transaction.Commit();
            return newOffers.ToList();
        }

        public async Task DoBackgroundWork()
        {
            while (!_ct.IsCancellationRequested)
            {
                try
                {
                    List<Offer> newOffers = await GetNewOffers();

                    if (newOffers.Any())
                    {
                        string message = BuildOfferMessage(newOffers);

                        await _notificationService.SendNotificationToAllUsers(message, _ct);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(5), _ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await Task.Delay(60000, _ct); // пауза 1 минута при ошибке
                }
            }
        }
        private string BuildOfferMessage(List<Offer> offers)
        {
            var sb = new StringBuilder();
            sb.AppendLine("🔥 <b>Новые акции!</b> 🔥\n");

            foreach (var offer in offers)
            {
                sb.AppendLine($"<b>{offer.Name}</b>");
                sb.AppendLine($"💰 Скидка: {100 * offer.Discount}%");
                if (!string.IsNullOrEmpty(offer.Description))
                    sb.AppendLine($"📝 {offer.Description}");
                sb.AppendLine($"📅 Действует до: {offer.EndsAt:dd.MM.yyyy}");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
