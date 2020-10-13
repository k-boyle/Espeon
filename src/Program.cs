using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace Espeon {
    internal class Program {
        private const string DefaultConfigDir = "./config.json";
        
        private static void WriteEspeonAscii() {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(Constants.EspeonAscii);
            Console.ForegroundColor = default;
        }

        private static async Task Main(string[] args) {
            WriteEspeonAscii();
            var configDir = args.Length > 0 ? args[0] : DefaultConfigDir;
            var config = await Config.FromJsonFileAsync(configDir);
            var logger = LoggerFactory.Create(config);
            // todo refactor to use mslogging and msconfig
            var host = CreateHostBuilder(args, logger, config).Build();
            using var scope = host.Services.CreateScope();
            await using (var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>()) {
                logger.ForContext("SourceContext", nameof(Program)).Information("Migrating database...");
                await context.Database.MigrateAsync();
            } 
            await host.RunAsync();
        }
        
        private static IHostBuilder CreateHostBuilder(string[] args, ILogger logger, Config config) {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => {
                    builder.AddFilter(level => false);
                })
                .ConfigureServices(serviceCollection => {
                    serviceCollection.AddSingleton(provider => {
                            var logger = provider.GetRequiredService<ILogger>();
                            var config = provider.GetRequiredService<Config>();
                            var prefixProvider = new EspeonPrefixProvider(provider.GetRequiredService<PrefixService>());
                            var botConfig = new DiscordBotConfiguration {
                                ProviderFactory = _ => provider,
                                Logger = new Optional<Disqord.Logging.ILogger>(
                                    LoggerFactory.CreateAdaptedLogger(logger))
                            };
                            return new EspeonBot(logger, config.Discord.Token, prefixProvider, botConfig);
                        })
                        .AddSingleton(logger)
                        .AddSingleton(config)
                        .AddSingleton<PrefixService>()
                        .AddSingleton<EspeonScheduler>()
                        .AddSingleton<ILocalisationProvider, PropertyBasedLocalisationProvider>()
                        .AddHostedService<EspeonService>()
                        .AddTHostedService<LocalisationService>()
                        .AddTHostedService<ReminderService>()
                        .AddDbContext<EspeonDbContext>(
                            (provider, options) => options.UseNpgsql(provider.GetRequiredService<Config>().Postgres.ConnectionString),
                            optionsLifetime: ServiceLifetime.Singleton);
                });
        }
    }
}
