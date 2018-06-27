using System.Threading.Tasks;
using Umbreon.Services;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Umbreon.Core
{
    class Core
    {
        private static async Task Main()
        {
            var services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 20
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig()
                {
                    LogLevel = LogSeverity.Verbose,
                    CaseSensitiveCommands = false
                }))
                .AddSingleton<EventsService>()
                .AddSingleton<LogService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<DatabaseService>()
                .AddSingleton<MessageService>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<TagService>()
                .AddSingleton<CustomCommandsService>()
                .BuildServiceProvider();

            await services.GetService<StartupService>().InitialiseAsync();
        }
    }
}
