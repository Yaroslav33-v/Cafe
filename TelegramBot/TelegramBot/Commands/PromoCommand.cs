using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Core;

namespace TelegramBot.Commands
{
    internal class PromoCommand : ITelegramCommand
    {
        public string CommandName => "/promo";

        public async Task Execute(Message message, ITelegramBotClient botClient, DatabaseProcessor processor, CancellationToken ct)
        {
            var promocode = await processor.GetPromocode();

            if (promocode == null)
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    "😔 <b>К сожалению, сейчас нет доступных промокодов.</b>\n" +
                    "Попробуйте позже или следите за нашими акциями /offers",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct
                );
                return;
            }

            var messageText =
                $"🎉 <b>Ваш персональный промокод!</b> 🎉\n\n" +
                $"<code>{promocode.Code}</code>\n\n" +
                $"💸 <b>Скидка:</b> {promocode.Discount} ₽\n" +
                $"💰 <b>Минимальная сумма заказа:</b> {promocode.FromSum} ₽\n" +
                $"⏳ <b>Действует до:</b> {promocode.ExpiresAt:dd.MM.yyyy}\n\n" +
                $"✨ <i>Скопируйте промокод и используйте при оформлении заказа</i>";

            await botClient.SendMessage(
                message.Chat.Id,
                messageText,
                parseMode: ParseMode.Html,
                cancellationToken: ct
            );
        }
    }
}
