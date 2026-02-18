using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Core;
using TelegramBot.Entities;

namespace TelegramBot.Commands
{
    internal class OfferCommand : ITelegramCommand
    {
        public string CommandName => "/offers";

        public async Task Execute(Message message, ITelegramBotClient botClient, DatabaseProcessor processor, CancellationToken ct)
        {
            var offers = await processor.GetOffers();

            if (!offers.Any())
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    "😔 На данный момент нет активных акций.\nСледите за обновлениями!",
                    cancellationToken: ct
                );
                return;
            }

            var messageText = BuildOffersMessage(offers);

            await botClient.SendMessage(
                message.Chat.Id,
                messageText,
                parseMode: ParseMode.Html,
                cancellationToken: ct
            );
        }

        private string BuildOffersMessage(List<Offer> offers)
        {
            var sb = new StringBuilder();

            // Заголовок
            sb.AppendLine("🎉 <b>Актуальные акции и скидки!</b> 🎉");
            sb.AppendLine();

            // Перебираем все акции
            for (int i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];

                if (i > 0)
                {
                    sb.AppendLine("⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯");
                    sb.AppendLine();
                }

                sb.AppendLine($"<b>🔥 {offer.Name}</b>");
                sb.AppendLine($"<i>{offer.Description}</i>");
                sb.AppendLine($"💰 Скидка: <b>{100 * offer.Discount}%</b>");
                
                var dates = new List<string>
                {
                    $"с {offer.StartsAt:dd.MM.yyyy}",
                    $"по {offer.EndsAt:dd.MM.yyyy}"
                };
                sb.AppendLine($"📅 Действует: {string.Join(" ", dates)}");

                sb.AppendLine(); // отступ после акции
            }

            // Подвал
            sb.AppendLine("⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯");
            sb.Append("💡 Чтобы узнать больше, введите /help");

            return sb.ToString();
        }
    }
}
