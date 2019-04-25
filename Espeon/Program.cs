using Casino.Common;
using Casino.Common.DependencyInjection;
using Casino.Common.Qmmands;
using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Espeon.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon
{
    internal class Program
    {
        private static void Main()
        {
            using var cts = new CancellationTokenSource();
            new Program().MainAsync(cts).GetAwaiter().GetResult();
        }

        private async Task MainAsync(CancellationTokenSource cts)
        {
            var config = Config.Create("./config.json");

            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.GetTypes()
                .Where(x => typeof(BaseService<InitialiseArgs>).IsAssignableFrom(x) && !x.IsAbstract).ToArray();

            var services = ConfigureServices(types, assembly, config, cts);

            using (var userStore = services.GetService<UserStore>()) //provides a scope for the variables
            {
                using var guildStore = services.GetService<GuildStore>();
                using var commandStore = services.GetService<CommandStore>();

                await userStore.Database.MigrateAsync();
                await guildStore.Database.MigrateAsync();
                await commandStore.Database.MigrateAsync();

                await services.RunInitialisersAsync(new InitialiseArgs
                {
                    UserStore = userStore,
                    GuildStore = guildStore,
                    CommandStore = commandStore
                }, types);

                await userStore.SaveChangesAsync();
                await guildStore.SaveChangesAsync();
                await commandStore.SaveChangesAsync();

                var espeon = new BotStartup(services, config);
                services.Inject(espeon);
                await espeon.StartAsync(userStore, commandStore);
            }

            await Task.Delay(-1, cts.Token);
        }

        private static IServiceProvider ConfigureServices(Type[] types, Assembly assembly,
            Config config, CancellationTokenSource cts)
        {
            return new ServiceCollection()
                .AddServices(types)
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 100
                }))
                .AddSingleton(new CommandService(new CommandServiceConfiguration
                {
                    StringComparison = StringComparison.InvariantCultureIgnoreCase,
                    CooldownBucketKeyGenerator = (obj, ctx, services) =>
                    {
                        var context = (EspeonContext)ctx;
                        return context.User.Id;
                    }
                })
                    .AddTypeParsers(assembly))
                .AddSingleton(config)
                .AddSingleton(cts)
                .AddSingleton(TaskQueue.Create())
                .AddSingleton<Random>()
                .AddConfiguredHttpClient()
                .AddEntityFrameworkNpgsql()
                .AddDbContext<UserStore>(ServiceLifetime.Transient)
                .AddDbContext<GuildStore>(ServiceLifetime.Transient)
                .AddDbContext<CommandStore>(ServiceLifetime.Transient)
                .BuildServiceProvider();
        }
    }
}
