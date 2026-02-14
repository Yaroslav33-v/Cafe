using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;
        static void Main()
        {
            using var cts = new CancellationTokenSource();
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates =
                [
                    UpdateType.Message,
                ],
                DropPendingUpdates = true,
            };

            Console.ReadLine();
        }
    }
}
