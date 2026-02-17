using Dapper;
using Npgsql;
using TelegramBot.Entities;

namespace TelegramBot.Core
{
    internal class DatabaseProcessor
    {
        private readonly NpgsqlConnection _connection = null!;

        public DatabaseProcessor(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
        }

        public async Task<Promocode?> GetPromocode()
        {
            Promocode? promocode = await _connection.QueryFirstOrDefaultAsync<Promocode>("SELECT " +
                "promocode_id as Id, " +
                "code as Code, " +
                "from_sum as FromSum, " +
                "discount as Discount, " +
                "expires_at as ExpiresAt " +
                "FROM public.promocodes " +
                "WHERE expires_at > CURRENT_DATE " +
                "ORDER BY RANDOM() LIMIT 1"
            );

            return promocode;
        }

        public async Task<List<Offer>> GetOffers()
        {
            IEnumerable<Offer> offers = await _connection.QueryAsync<Offer>("SELECT " +
                "offer_id AS Id, " +
                "offer_name AS Name, " +
                "description AS Description, " +
                "discount AS Discount, " +
                "starts_at AS StartsAt, " +
                "ends_at AS EndsAt, " +
                "is_notificated AS IsNotificated " +
                "FROM public.offers " +
                "WHERE CURRENT_DATE >= starts_at " +
                "AND CURRENT_DATE <= ends_at");

            return offers.ToList();
        }
    }
}
