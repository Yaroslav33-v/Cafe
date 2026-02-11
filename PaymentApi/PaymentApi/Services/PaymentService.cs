using Dapper;
using Npgsql;
using PaymentApi.Dto;
using System.Data;

namespace PaymentApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IDbConnection _connection;
        public PaymentService(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task TryToPay(PaymentDto paymentDto)
        {
            const string sql = "CALL public.do_payment_attempt(@p_card_token, @p_last_four, @p_order_total, @p_description)";

            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@p_card_token", paymentDto.CardToken);
                parameters.Add("@p_last_four", paymentDto.LastFour);
                parameters.Add("@p_order_total", paymentDto.Total);
                parameters.Add("@p_description", paymentDto.Description);

                await _connection.ExecuteAsync(sql, parameters);
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("Недостаточно средств"))
            {
                throw new Exception("Недостаточно средств");
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("Карта с токеном"))
            {
                throw new Exception("Карта не найдена");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
