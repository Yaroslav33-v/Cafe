using CafeWeb.Models;
using Dapper;
using System.Data;

namespace CafeWeb.Services
{
    public class PromocodeService : IPromocodeService
    {
        private readonly IDbConnection _connection;

        public PromocodeService(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }
        public async Task<Promocode> GetPromocodeInfo(string code)
        {
            string sql = @"SELECT 
	                        from_sum AS FromSum,
	                        discount AS Discount
                        FROM public.promocodes 
                        WHERE CURRENT_DATE < expires_at 
                        AND code = @Code;";

            try
            {
                var promocodeInfo = await _connection.QueryFirstAsync<Promocode>(sql, new { code });
                return promocodeInfo;
            }
            catch
            {
                throw new Exception("Внутренняя ошибка базы данных");
            }
        }
    }
}
