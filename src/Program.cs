using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            // todo refactor to use mslogging and msconfig
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            await host.RunAsync();
        }
        
        private static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => {
                    builder.AddFilter(level => false);
                })
                .ConfigureAppConfiguration(configurationBuilder => {
                    var configDir = configurationBuilder.Build()["config"] ?? DefaultConfigDir;
                    configurationBuilder.AddJsonFile(configDir);
                })
                .ConfigureServices((hostContext, serviceCollection) => {
                    var configuration = hostContext.Configuration;
                    serviceCollection.AddSingleton(Espeon)
                        .AddSingleton(Logger)
                        .AddSingleton<PrefixService>()
                        .AddSingleton<EspeonScheduler>()
                        .AddSingleton<ILocalisationProvider, PropertyBasedLocalisationProvider>()
                        .AddHostedService<EspeonService>()
                        .AddFetchableHostedService<LocalisationService>()
                        .AddFetchableHostedService<ReminderService>()
                        .AddDbContext<EspeonDbContext>(UseNpgsql, optionsLifetime: ServiceLifetime.Singleton)
                        .ConfigureSection<Discord>(configuration)
                        .ConfigureSection<Localisation>(configuration)
                        .ConfigureSection<Logging>(configuration)
                        .ConfigureSection<Postgres>(configuration);
                });
        }

        private static EspeonBot Espeon(IServiceProvider provider) {
            var logger = provider.GetRequiredService<ILogger>();
            var prefixProvider = new EspeonPrefixProvider(provider.GetRequiredService<PrefixService>());
            var botConfig = new DiscordBotConfiguration {
                ProviderFactory = _ => provider,
                Logger = new Optional<Disqord.Logging.ILogger>(LoggerFactory.CreateAdaptedLogger(logger))
            };
            return new EspeonBot(logger, provider.GetRequiredService<IOptions<Discord>>(), prefixProvider, botConfig);
        }

        private static ILogger Logger(IServiceProvider provider) {
            var loggingOptions = provider.GetRequiredService<IOptions<Logging>>();
            return LoggerFactory.Create(loggingOptions);
        }

        private static void UseNpgsql(IServiceProvider provider, DbContextOptionsBuilder options) {
            options.UseNpgsql(provider.GetRequiredService<IOptions<Postgres>>().Value.ConnectionString);
        }
    }
}
