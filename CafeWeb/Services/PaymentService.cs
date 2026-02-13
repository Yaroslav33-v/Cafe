using CafeWeb.Dto;
using CafeWeb.Models;
using System.Text;
using System.Text.Json;

namespace CafeWeb.Services
{
    public class PaymentService : IPaymentService
    {
        private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("https://localhost:7003") };
        private static string GeneratePaymentMethod(PaymentModel paymentModel)
        {
            return "pm_" + (paymentModel.CardNumber.Replace(" ", "")) + "_" + paymentModel.CvvCode;
        }
        public async Task<(bool, string?)> TryToPay(PaymentModel paymentModel)
        {
            try
            {
                string paymentMethod = GeneratePaymentMethod(paymentModel);
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
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/payment_attempt", content);

                if (response.IsSuccessStatusCode)
                    return (true, null);
                else
                    return (false, await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException)
            {
                return (false, "Ошибка оплаты");
            }
        }
    }
}

