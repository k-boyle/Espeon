using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    class Program
    {
        private static async Task Main()
        {
            var assembly = Assembly.GetEntryAssembly();
            var services = new ServiceCollection()
                .AddServices(assembly)
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 100
                }))
                .AddSingleton(new CommandService(new CommandServiceConfiguration
                {
                    CaseSensitive = false
                }))
                .BuildServiceProvider()
                .Inject(assembly)
                .RunInitialisers(assembly);

            var espeon = new EspeonStartup(services);
            services.Inject(espeon);
            await espeon.StartBotAsync();

            await Task.Delay(-1);
        }
    }
}
