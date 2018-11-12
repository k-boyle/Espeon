using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pusharp;
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
            var config = Config.Create("./config.json");

            var assembly = Assembly.GetEntryAssembly();
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
                .AddSingleton(new PushBulletClient(new PushBulletClientConfig
                {
                    LogLevel = LogLevel.Debug,
                    UseCache = true,
                    Token = config.PushbulletToken
                }))
                .AddSingleton<Random>()
                .BuildServiceProvider()
                .Inject(types)
                .RunInitialisers(types);

            var espeon = new EspeonStartup(services);
            services.Inject(espeon);
            await espeon.StartBotAsync();

            await Task.Delay(-1);
        }
    }
}
