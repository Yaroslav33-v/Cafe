using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Commands;
using TelegramBot.Core;

namespace TelegramBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient = null!;
        private static readonly List<ITelegramCommand> _commands = 
        [
            new StartCommand(),
            new OfferCommand(),
            new PromoCommand()
        ];
        private static readonly CommandDispatcher _dispatcher = new(_commands);
        private static ReceiverOptions _receiverOptions = null!;
        private static readonly string _connectionString = Config.GetDatabaseConnectionString();
        private static readonly DatabaseProcessor _processor = new(_connectionString);

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

            _botClient = new TelegramBotClient(Config.GetBotToken());
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cancellationToken: cts.Token);

            Console.WriteLine("Бот запущен, для остановки введите любой символ");
            Console.ReadLine();
        }

        private static Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private static async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            await _dispatcher.Dispatch(update.Message!.Text!.ToLower(), update.Message, client, _processor, token);
        }
    }
}
