using Casino.Common;
using Casino.DependencyInjection;
using Casino.Qmmands;
using Discord;
using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Espeon.Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Espeon.Commands;

namespace Espeon.Bot
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
            var types = assembly?.GetTypes()
                .Where(x => typeof(BaseService<InitialiseArgs>).IsAssignableFrom(x) && !x.IsAbstract).ToArray();

            var impls = new List<Type>();

            Type GetImpl(Type type)
            {
                var interfaces = type.GetInterfaces();
                var impl = Array.Find(interfaces, x => !typeof(IDisposable).IsAssignableFrom(x));

                impls.Add(impl);

                return impl;
            }

            var dict = types.ToDictionary(GetImpl, x => x);

            var services = ConfigureServices(dict, assembly, config, cts);

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
                }, impls);

                await userStore.SaveChangesAsync();
                await guildStore.SaveChangesAsync();
                await commandStore.SaveChangesAsync();

                var espeon = new BotStartup(services, config);
                services.Inject(espeon);
                await espeon.StartAsync(userStore, commandStore);
            }

            await Task.Delay(-1, cts.Token);
        }

        private static IServiceProvider ConfigureServices(IDictionary<Type, Type> types, Assembly assembly,
            Config config, CancellationTokenSource cts)
        {
            return new ServiceCollection()
                .AddServices(types)
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    ExclusiveBulkDelete = true,
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 100
                }))
                .AddSingleton(new CommandService(new CommandServiceConfiguration
                {
                    StringComparison = StringComparison.InvariantCultureIgnoreCase,
                    CooldownBucketKeyGenerator = (_, ctx, __) =>
                    {
                        var context = (EspeonContext)ctx;
                        return context.User.Id;
                    }
                })
                    .AddTypeParsers(assembly))
                .AddSingleton(config)
                .AddSingleton(cts)
                .AddSingleton(new TaskQueue(20))
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
