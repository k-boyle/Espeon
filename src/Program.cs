using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon {
    internal class Program {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        private static async Task Main(string[] args) {
            var config = await Config.FromJsonFileAsync("./config.json");
            var logger = LoggerFactory.Create(config);
            var services = CreateServiceProvider(logger, config);
            var program = new Program(logger, services);
            await program.StartAsync();
        }

        private Program(ILogger logger, IServiceProvider services) {
            this._services = services;
            this._logger = logger.ForContext("SourceContext", typeof(Program).Name);
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

        //TODO config from args

        private static ServiceProvider CreateServiceProvider(ILogger logger, Config config) {
           return new ServiceCollection()
                .AddSingleton(provider => {
                    var logger = provider.GetService<ILogger>();
                    var config = provider.GetService<Config>();
                    var prefixProvider = new EspeonPrefixProvider(provider.GetService<PrefixService>());
                    var botConfig = new DiscordBotConfiguration { ProviderFactory = _ => provider };
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
                this._logger.Information("Initialising {Service}", service.GetType().Name);
                await service.InitialiseAsync();
            }
        }
        
        private async Task RunEspeonAsync() {
            await using var espeon = this._services.GetService<EspeonBot>();
            await espeon.RunAsync();
        }
    }
}
