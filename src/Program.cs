using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Espeon {
    internal class Program {
        private const string DefaultConfigDir = "./config.json";
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        private static async Task Main(string[] args) {
            var configDir = args.Length > 0 ? args[0] : DefaultConfigDir;
            var config = await Config.FromJsonFileAsync(configDir);
            var logger = LoggerFactory.Create(config);
            var services = CreateServiceProvider(logger, config);
            var program = new Program(logger, services);
            await program.StartAsync();
        }

        private Program(ILogger logger, IServiceProvider services) {
            this._services = services;
            this._logger = logger.ForContext("SourceContext", nameof(Program));
        }

        private async Task StartAsync() {
            this._logger.Information("Starting Espeon...");
            using (var scope = this._services.CreateScope()) {
                await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
                this._logger.Information("Migrating database...");
                await context.Database.MigrateAsync();
            }

            await InitialiseServicesAsync();
            await RunEspeonAsync();
        }

        private static ServiceProvider CreateServiceProvider(ILogger logger, Config config) {
           return new ServiceCollection()
                .AddSingleton(provider => {
                    var logger = provider.GetService<ILogger>();
                    var config = provider.GetService<Config>();
                    var prefixProvider = new EspeonPrefixProvider(provider.GetService<PrefixService>());
                    var botConfig = new DiscordBotConfiguration {
                        ProviderFactory = _ => provider,
                        Logger = new Optional<Disqord.Logging.ILogger>(LoggerFactory.CreateAdaptedLogger(logger))
                    };
                    return new EspeonBot(logger, config.Discord.Token, prefixProvider, botConfig); 
                })
                .AddSingleton(logger)
                .AddSingleton(config)
                .AddSingleton<PrefixService>()
                .AddSingleton<EspeonScheduler>()
                .AddInitialisableSingleton<LocalisationService>()
                .AddOnReadySingleton<ReminderService>()
                .AddDbContext<EspeonDbContext>(options => options.UseNpgsql(config.Postgres.ConnectionString))
                .BuildServiceProvider();
        }

        private async Task InitialiseServicesAsync() {
            foreach (var service in this._services.GetServices<IInitialisableService>()) {
                this._logger.Information("Initialising {service}", service.GetType().Name);
                await service.InitialiseAsync();
            }
        }
        
        private async Task RunEspeonAsync() {
            await using var espeon = this._services.GetService<EspeonBot>();
            await espeon.RunAsync();
        }
    }
}
