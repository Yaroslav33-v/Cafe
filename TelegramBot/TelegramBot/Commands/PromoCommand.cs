using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Core;

namespace TelegramBot.Commands
{
    internal class PromoCommand : ITelegramCommand
    {
        public string CommandName => "/promo";

        public async Task Execute(Message message, ITelegramBotClient botClient, DatabaseProcessor processor, CancellationToken ct)
        {
            var promocode = await processor.GetPromocode();
            await botClient.SendMessage(message.Chat.Id, "Промокод:" + promocode?.Code, cancellationToken: ct);
        }
    }
}
