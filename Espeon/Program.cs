using Discord;
using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Espeon.Extensions;
using Espeon.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    internal class Program
    {
        private static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            var config = Config.Create("./config.json");

            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.GetTypes()
                .Where(x => typeof(BaseService).IsAssignableFrom(x) && !x.IsAbstract).ToArray();

            var services = ConfigureServices(types, assembly, config);

            using (var userStore = services.GetService<UserStore>()) //provides a scope for the variables
            {
                using var guildStore = services.GetService<GuildStore>();
                using var commandStore = services.GetService<CommandStore>();

                await userStore.Database.MigrateAsync();
                await guildStore.Database.MigrateAsync();
                await commandStore.Database.MigrateAsync();

                await services.RunInitialisersAsync(userStore, guildStore, commandStore, types);

                await userStore.SaveChangesAsync();
                await guildStore.SaveChangesAsync();
                await commandStore.SaveChangesAsync();

                var espeon = new BotStartup(services, config);
                services.Inject(espeon);
                await espeon.StartAsync(userStore, commandStore);
            }

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices(IEnumerable<Type> types, Assembly assembly, Config config)
        {
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
                .AddSingleton(config)
                .AddSingleton<Random>()
                .AddConfiguredHttpClient()
                .AddEntityFrameworkNpgsql()
                .AddDbContext<UserStore>(ServiceLifetime.Transient)
                .AddDbContext<GuildStore>(ServiceLifetime.Transient)
                .AddDbContext<CommandStore>(ServiceLifetime.Transient)
                .BuildServiceProvider()
                .Inject(types);

            return services;
        }
    }
}
