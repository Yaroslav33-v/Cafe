using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Commands;

namespace TelegramBot.Core
{
    internal class CommandDispatcher
    {
        private readonly Dictionary<string, ITelegramCommand> _commands = null!;

        public CommandDispatcher(IEnumerable<ITelegramCommand> commands)
        {
            _commands = commands.ToDictionary(c => c.CommandName);
        }

        public async Task Dispatch(string command, Message message, ITelegramBotClient botClient, DatabaseProcessor processor, CancellationToken ct)
        {
            if(_commands.TryGetValue(command, out var cmd))
            {
                await cmd.Execute(message, botClient, processor, ct);
            }
            else
            {
                await botClient.SendMessage(message.Chat.Id, "Бот не знает такой команды!", cancellationToken: ct);
            }
        }
    }
}
