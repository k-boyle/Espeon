using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon {
    class Program {
        //TODO config from args
        static async Task Main(string[] args) {
            var config = await Config.FromJsonFileAsync("./config.json");
            var logger = LoggerFactory.Create(config);
            var programLogger = logger.ForContext("SourceContext", typeof(Program).Name);
            programLogger.Information("Starting Espeon...");
            
            var services = CreateServiceProvider(logger, config);
            
            using (var scope = services.CreateScope()) {
                await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
                programLogger.Information("Migrating database...");
                await context.Database.MigrateAsync();
            }

            await InitialiseServicesAsync(programLogger, services);
            await RunEspeonAsync(services);
        }

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

        private static async Task InitialiseServicesAsync(ILogger logger, IServiceProvider services) {
            foreach (var service in services.GetServices<IInitialisableService>()) {
                logger.Information("Initialising {Service}", service.GetType().Name);
                await service.InitialiseAsync();
            }
        }
        
        private static async Task RunEspeonAsync(IServiceProvider services) {
            await using var espeon = services.GetService<EspeonBot>();
            espeon.AddModules(Assembly.GetEntryAssembly());
            await espeon.RunAsync();
        }
    }
}
