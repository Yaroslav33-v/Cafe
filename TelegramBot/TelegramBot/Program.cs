using Telegram.Bot;
using Telegram.Bot.Exceptions;
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
            new PromoCommand(),
            new HelpCommand()
        ];
        private static readonly CommandDispatcher _dispatcher = new(_commands);
        private static ReceiverOptions _receiverOptions = null!;
        private static readonly string _connectionString = Config.GetDatabaseConnectionString();
        private static readonly DatabaseProcessor _processor = new(_connectionString);

        static void Main()
        {
            Config.ConfigureNLog();
            try
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

                AppLogger.LogInfo("Бот успешно запущен");

                BackgroundWorker backgroundWorker = new(_connectionString, _botClient, cts.Token);
                Task.Run(backgroundWorker.DoBackgroundWork);

                Console.WriteLine("Бот запущен, для остановки введите любой символ");
                Console.ReadLine();

                cts.Cancel();
                AppLogger.LogInfo("Бот остановлен");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Критическая ошибка при запуске ", ex);
                Console.WriteLine("Критическая ошибка при запуске ", ex);
                Console.ReadLine();
            }
        }

        private static Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiEx =>
                    $"Telegram API Error:\n[{apiEx.ErrorCode}]\n{apiEx.Message}",

                HttpRequestException httpEx =>
                    $"HTTP Error: {httpEx.Message}",

                _ =>
                    $"System Error: {exception.GetType()}\n{exception.Message}\n{exception.StackTrace}"
            };

            Console.WriteLine($"Error: {errorMessage}");
            AppLogger.LogError($"Error: {errorMessage}");
            return Task.CompletedTask;
        }

        private static async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var userService = new UserService(_connectionString);

            await userService.RegisterUserInteraction(update.Message);

            await _dispatcher.Dispatch(update.Message!.Text!.ToLower(), update.Message, client, _processor, token);
        }
    }
}
