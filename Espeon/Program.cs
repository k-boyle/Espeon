using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Attributes;
using Espeon.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    class Program
    {
        private static async Task Main()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = assemblies.SingleOrDefault(x => x.GetName().Name == "Espeon.Implementation");

            var types = assembly.FindTypesWithAttribute<ServiceAttribute>()
                .Where(x => x.GetCustomAttribute<ServiceAttribute>().Implement).ToImmutableArray();

            var services = new ServiceCollection()
                .AddServices(types)
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 100
                }))
                .AddSingleton(new CommandService(new CommandServiceConfiguration
                {
                    CaseSensitive = false
                })
                    .AddTypeParsers(assembly))
                .AddSingleton<Random>()
                .BuildServiceProvider()
                .Inject(types)
                .RunInitialisers(types);

            var espeon = new EspeonStartup<EspeonContext>(services);
            services.Inject(espeon);
            await espeon.StartBotAsync();

            await Task.Delay(-1);
        }
    }
}
