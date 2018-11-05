using System.Collections.Immutable;
using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System.Reflection;
using System.Threading.Tasks;
using Espeon.Core.Attributes;

namespace Espeon
{
    class Program
    {
        private static async Task Main()
        {
            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.FindTypesWithAttribute<ServiceAttribute>().ToImmutableArray();

            var services = new ServiceCollection()
                .AddServices(assembly, types)
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
                .Inject(assembly, types)
                .RunInitialisers(assembly, types);

            var espeon = new EspeonStartup(services);
            services.Inject(espeon);
            await espeon.StartBotAsync();

            await Task.Delay(-1);
        }
    }
}
