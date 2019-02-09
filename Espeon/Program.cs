using Discord;
using Discord.WebSocket;
using Espeon.Database;
using Espeon.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pusharp;
using Qmmands;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    internal class Program
    {
        private static async Task Main()
        {
            var config = Config.Create("./config.json");

            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.GetTypes()
                .Where(x => typeof(BaseService).IsAssignableFrom(x) && !x.IsAbstract).ToArray();

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
                .AddSingleton(config)
                .AddSingleton<Random>()
                .AddEntityFrameworkNpgsql()
                .AddDbContext<DatabaseContext>(ServiceLifetime.Transient)
                .BuildServiceProvider()
                .Inject(types);

            var ctx = services.GetService<DatabaseContext>();

            await ctx.Database.MigrateAsync();

            await services.RunInitialisersAsync(ctx, types);

            await ctx.SaveChangesAsync();

            var espeon = new EspeonStartup(services, config);
            services.Inject(espeon);
            await espeon.StartBotAsync(ctx);

            await Task.Delay(-1);
        }
    }
}
