using CafeWeb.CustomExceptions;
using CafeWeb.Dto;
using CafeWeb.Models;
using System.Text;
using System.Text.Json;

namespace CafeWeb.Services
{
    public class PaymentService : IPaymentService
    {
        private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("https://localhost:7003") };
        private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
        private readonly ILogger<PaymentService> _logger;
        private readonly IPromocodeService _promocodeService;

        public PaymentService(ILogger<PaymentService> logger, IPromocodeService promocodeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _promocodeService = promocodeService ?? throw new ArgumentNullException(nameof(promocodeService));
        }
        private static string GeneratePaymentMethod(PaymentModel paymentModel) =>
            "pm_" + paymentModel.CardNumber.Replace(" ", "") + "_" + paymentModel.CvvCode;

        private static async Task<string> GetAuthToken(string cardToken)
        {
            // Подключаемся к эндпоинту авторизации
            var response = await _httpClient.GetAsync($"/login/{cardToken}");
            response.EnsureSuccessStatusCode();
            
            // Читаем ответ
            string json = await response.Content.ReadAsStringAsync();
            PaymentAuthDto authDto = JsonSerializer.Deserialize<PaymentAuthDto>(json, _options)
                ?? throw new HttpRequestException();

            return authDto.Token;
        }

        public async Task<bool> TryToPay(PaymentModel paymentModel)
        {
            try
            {
                string paymentMethod = GeneratePaymentMethod(paymentModel);

                // Устанавливаем значение по умолчанию для total
                paymentModel.TotalAmount = paymentModel.OriginalAmount;

                // Получаем данные промокода для расчета скидки на бэке
                if (paymentModel.Promocode is string promo)
                {
                    Promocode? code = await _promocodeService.GetPromocodeInfo(promo);

                    if (code is not null && paymentModel.OriginalAmount > code.FromSum && DateTime.Now < code.ExpiresAt)
                    {
                        paymentModel.TotalAmount = paymentModel.OriginalAmount - code.Discount;
                    }
                }

                // Создаем объект для отправки в апи
                PaymentDto paymentDto = new
                (
                    CardToken: paymentMethod,
                    LastFour: paymentModel.CardNumber[^4..],
                    ExpMonth: int.Parse(paymentModel.MonthYear[0..2]),
                    ExpYear: 2000 + int.Parse(paymentModel.MonthYear[^2..]),
                    Total: paymentModel.TotalAmount,
                    Description: "Оплата заказа в кафе"
                );

                string json = JsonSerializer.Serialize(paymentDto);

                // Получаем jwt
                string jwt = await GetAuthToken(paymentDto.CardToken);

                // Создаем HTTP запрос
                using var request = new HttpRequestMessage(HttpMethod.Post, "/payment_attempt");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                // Читаем ответ от апи
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Платеж для карты {LastFour} успешно выполнен",
                        paymentModel.CardNumber[^4..]);

                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    _logger.LogInformation("Платеж для карты {LastFour} не выполнен, ошибка со стороны API",
                        paymentModel.CardNumber[^4..]);
                   
                    return false;
                }
                else
                {
                    _logger.LogInformation("Платеж для карты {LastFour} не выполнен",
                        paymentModel.CardNumber[^4..]);

                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Ошибка при подключении к API: {message} для карты {LastFour}",
                    ex.Message, paymentModel.CardNumber[^4..]);
                throw new PaymentFailedException("Ошибка при подключении к платежной системе");
            }
            catch(Exception ex)
            {
                _logger.LogError("Ошибка при обработке платежа: {message} для карты {LastFour}",
                    ex.Message, paymentModel.CardNumber[^4..]);
                throw new PaymentFailedException("Ошибка при обработке платежа");
            }
        }
    }
}

