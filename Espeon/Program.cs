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
    class Program
    {
        private static async Task Main()
        {
            var config = Config.Create("./config.json");

            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.GetTypes()
                .Where(x => typeof(IService).IsAssignableFrom(x) && !x.IsInterface).ToArray();

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
                .AddDbContext<DatabaseContext>()
                .BuildServiceProvider()
                .Inject(types);

            using (var scope = services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetService<DatabaseContext>();

                await ctx.Database.MigrateAsync();

                await services.RunInitialisersAsync(ctx, types);

                var espeon = new EspeonStartup(services, config);
                services.Inject(espeon);
                await espeon.StartBotAsync(ctx);

                await ctx.SaveChangesAsync();
            }

            await Task.Delay(-1);
        }
    }
}
