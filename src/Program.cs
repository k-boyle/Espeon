using Disqord;
using Espeon.DisqordImplementations;
using Espeon.Persistence;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Espeon {
    class Program {
        //TODO config from args
        static async Task Main(string[] args) {
            var config = await Config.FromJsonFileAsync("./config.json");
            var services = new ServiceCollection()
                .AddSingleton(provider => {
                    var prefixProvider = new EspeonPrefixProvider(provider.GetService<PrefixService>());
                    var config = provider.GetService<Config>();
                    var botConfig = new DiscordBotConfiguration {
                        ProviderFactory = _ => provider
                    };
                    return new EspeonBot(config.Discord.Token, prefixProvider, botConfig);
                })
                .AddSingleton(config)
                .AddSingleton<PrefixService>()
                .AddTransient<EspeonDbContext>()
                .BuildServiceProvider();
            await using var espeon = services.GetService<EspeonBot>();
            await espeon.RunAsync();
        }
    }
}
