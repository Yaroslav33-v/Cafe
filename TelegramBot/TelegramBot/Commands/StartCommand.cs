using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Core;

namespace TelegramBot.Commands
{
    internal class StartCommand : ITelegramCommand
    {
        public string CommandName => "/start";

        public async Task Execute(Message message, ITelegramBotClient botClient, DatabaseProcessor processor, CancellationToken ct)
        {
            await botClient.SendMessage(message.Chat.Id, "Привет!", cancellationToken: ct);
        }
    }
}
