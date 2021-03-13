using System;
using Disqord.Bot.Hosting;
using Espeon.Logging;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Espeon
{
    public class Program
    {
        private const string CONFIG = "./config.yml";

        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(Constants.EspeonAscii);
            Console.ForegroundColor = default;

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog((context, logger) =>
                {
                    logger.ReadFrom.Configuration(context.Configuration, "serilog")
                        .WriteTo.Console(outputTemplate: LoggerTemplate.CONSOLE, theme: EspeonLoggingConsoleTheme.Instance)
                        .WriteTo.File("./logs/log-.txt", outputTemplate: LoggerTemplate.FILE, rollingInterval: RollingInterval.Day)
                        .Enrich.With<ClassNameEnricher>();
                })
                .ConfigureServices(services => {})
                .ConfigureAppConfiguration(configuration => configuration.AddYamlFile(CONFIG))
                .ConfigureDiscordBot((context, bot) =>
                {
                    bot.Token = context.Configuration["discord:token"];
                    bot.Prefixes = new[] { "ts" };
                    bot.UseMentionPrefix = true;
                })
                .Build();

            using (host)
            {
                host.Run();
            }
        }
    }
}