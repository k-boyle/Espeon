using System;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Umbreon.Attributes;
using Umbreon.Services;

namespace Umbreon.Core
{
    class Core
    {
        private static async Task Main()
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 20
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    CaseSensitiveCommands = false
                }))
                .AddSingleton<InteractiveService>();

            var attribute = typeof(ServiceAttribute);
            var services = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(y => y.GetCustomAttributes(attribute, true).Length > 0);
            foreach (var service in services)
                serviceCollection.AddSingleton(service);

            var builtProvider = serviceCollection.BuildServiceProvider();

            await builtProvider.GetService<StartupService>().InitialiseAsync();
        }
    }
}
