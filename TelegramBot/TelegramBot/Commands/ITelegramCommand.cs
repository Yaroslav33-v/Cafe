using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Commands
{
    internal interface ITelegramCommand
    {
        string CommandName { get; }
        Task Execute(Message message, ITelegramBotClient botClient, CancellationToken ct);
    }
}
