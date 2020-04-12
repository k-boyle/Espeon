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
            logger.ForContext("SourceContext", typeof(Program).Name).Information("Starting Espeon...");
            
            var services = CreateServiceProvider(logger, config);
            
            await using (var context = services.GetService<EspeonDbContext>()) {
                await context.Database.MigrateAsync();
            }

            await InitialiseServicesAsync(services);
            await RunEspeonAsync(services);
        }

        private static ServiceProvider CreateServiceProvider(ILogger logger, Config config) {
           return new ServiceCollection()
                .AddSingleton(provider => {
                    var prefixProvider = new EspeonPrefixProvider(provider.GetService<PrefixService>());
                    var botConfig = new DiscordBotConfiguration { ProviderFactory = _ => provider };
                    return new EspeonBot(logger, config.Discord.Token, prefixProvider, botConfig); 
                })
                .AddSingleton(logger)
                .AddSingleton(config)
                .AddSingleton<PrefixService>()
                .AddInitialisableSingleton<LocalisationService>()
                .AddTransient<EspeonDbContext>()
                .BuildServiceProvider();
        }

        private static async Task InitialiseServicesAsync(IServiceProvider services) {
            foreach (var service in services.GetServices<IInitialisableService>()) {
                await service.InitialiseAsync();
            }
        }
        
        private static async Task RunEspeonAsync(ServiceProvider services) {
            await using var espeon = services.GetService<EspeonBot>();
            espeon.AddModules(Assembly.GetEntryAssembly());
            await espeon.RunAsync();
        }
    }
}
