using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Core;

namespace TelegramBot.Commands
{
    internal interface ITelegramCommand
    {
        string CommandName { get; }
        Task Execute(Message message, ITelegramBotClient botClient, DatabaseProcessor processor, CancellationToken ct);
    }
}
