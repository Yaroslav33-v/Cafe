using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;

namespace TelegramBot
{
    internal class Config
    {
        public static string GetDatabaseConnectionString()
        {
            var basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return configuration.GetConnectionString("DefaultConnection");
        }
        public static void ConfigureNLog()
        {
            var basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var nlogConfig = configuration.GetSection("NLog");

            if (nlogConfig.Exists())
            {
                LogManager.Configuration = new NLogLoggingConfiguration(nlogConfig);
            }
        }
        public static string GetBotToken()
        {
            var basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return configuration.GetSection("BotConfiguration:BotToken").Value;
        }
    }
}
