using Telegram.Bot;
using Telegram.Bot.Types;
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
            await botClient.SendMessage(message.Chat.Id, "Скидки: " + offers.First().Name, cancellationToken: ct);
        }
    }
}
