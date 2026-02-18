using NLog;

namespace TelegramBot
{
    internal static class AppLogger
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static void LogInfo(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        public static void LogError(string message, Exception? ex = null, params object[] args)
        {
            if (ex == null)
                _logger.Error(message, args);
            else
                _logger.Error(ex, message, args);
        }

        public static void LogWarning(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }

        public static void LogDebug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }
    }
}
