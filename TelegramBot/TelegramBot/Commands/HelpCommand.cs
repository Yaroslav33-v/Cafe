using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Core;

namespace TelegramBot.Commands
{
    internal class HelpCommand : ITelegramCommand
    {
        public string CommandName => "/help";
        public async Task Execute(Message message, ITelegramBotClient botClient, DatabaseProcessor processor, CancellationToken ct)
        {
            await botClient.SendMessage(
                message.Chat.Id,
                $"👋 <b>Привет, {message.Chat.FirstName}!</b>\n\n" +
                "<b>Доступные команды:</b>\n" +
                "• /offers — акции\n" +
                "• /promo — промокод\n" +
                "• /help — помощь",
                parseMode: ParseMode.Html,
                cancellationToken: ct
            );
        }
    }
}
