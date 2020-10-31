using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Espeon {
    internal class Program {
        private const string DefaultConfigDir = "./config.yml";
        
        private static void WriteEspeonAscii() {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(Constants.EspeonAscii);
            Console.ForegroundColor = default;
        }

        private static async Task Main(string[] args) {
            WriteEspeonAscii();
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            await host.RunAsync();
        }
        
        private static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog((hostContext, loggingConfiguration) => {
                    loggingConfiguration.ReadFrom.Configuration(hostContext.Configuration);
                })
                .ConfigureAppConfiguration(ConfigureConfig)
                .ConfigureServices((hostContext, serviceCollection) => {
                    var configuration = hostContext.Configuration;
                    serviceCollection.AddSingleton(Espeon)
                        .AddSingleton<PrefixService>()
                        .AddSingleton<EspeonScheduler>()
                        .AddSingleton<DisqordLogger>()
                        .AddSingleton<HttpClient>()
                        .AddSingleton<ILocalisationProvider, PropertyBasedLocalisationProvider>()
                        .AddOnReadyService<ReminderService>()
                        .AddHostedService<EspeonService>()
                        .AddFetchableHostedService<LocalisationService>()
                        .AddDbContext<EspeonDbContext>(UseNpgsql, optionsLifetime: ServiceLifetime.Singleton)
                        .ConfigureSection<Discord>(configuration)
                        .ConfigureSection<Localisation>(configuration)
                        .ConfigureSection<Postgres>(configuration)
                        .ConfigureSection<Emotes>(configuration);
                });
        }

        private static void ConfigureConfig(IConfigurationBuilder configurationBuilder) {
            var configDir = configurationBuilder.Build()["config"] ?? DefaultConfigDir;
            if (!File.Exists(configDir)) {
                throw new FileNotFoundException($"Missing config file {configDir}");
            }

            switch (Path.GetExtension(configDir)) {
                case ".yaml":
                case ".yml":
                    configurationBuilder.AddYamlFile(configDir);
                    break;

                case ".json":
                    configurationBuilder.AddJsonFile(configDir);
                    break;

                default:
                    throw new InvalidOperationException($"{Path.GetExtension(configDir)} is not a valid config type");
            }
        }

        private static EspeonBot Espeon(IServiceProvider provider) {
            var disqordLogger = provider.GetRequiredService<DisqordLogger>();
            var espeonLogger = provider.GetRequiredService<ILogger<EspeonBot>>();
            var prefixProvider = new EspeonPrefixProvider(provider.GetRequiredService<PrefixService>());
            var botConfig = new DiscordBotConfiguration {
                ProviderFactory = _ => provider,
                Logger = disqordLogger
            };
            return new EspeonBot(espeonLogger, provider.GetRequiredService<IOptions<Discord>>(), prefixProvider, botConfig);
        }

        private static void UseNpgsql(IServiceProvider provider, DbContextOptionsBuilder options) {
            options.UseNpgsql(provider.GetRequiredService<IOptions<Postgres>>().Value.ConnectionString);
        }
    }
}
