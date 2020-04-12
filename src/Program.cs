using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon {
    class Program {
        //TODO config from args
        static async Task Main(string[] args) {
            var config = await Config.FromJsonFileAsync("./config.json");
            var logger = LoggerFactory.Create(config);
            logger.Information("Starting Espeon...");
            
            var services = new ServiceCollection()
                .AddSingleton(provider => {
                    var prefixProvider = new EspeonPrefixProvider(provider.GetService<PrefixService>());
                    var botConfig = new DiscordBotConfiguration {
                        ProviderFactory = _ => provider
                    };
                    return new EspeonBot(logger, config.Discord.Token, prefixProvider, botConfig);
                })
                .AddSingleton(logger)
                .AddSingleton(config)
                .AddSingleton<PrefixService>()
                .AddSingleton<LocalisationService>()
                .AddTransient<EspeonDbContext>()
                .BuildServiceProvider();
            await using (var context = services.GetService<EspeonDbContext>()) {
                await context.Database.MigrateAsync();
            }
            await using var espeon = services.GetService<EspeonBot>();
            await services.GetService<LocalisationService>().InitialiseAsync();
            espeon.AddModules(Assembly.GetEntryAssembly());
            await espeon.RunAsync();
        }
    }
}
