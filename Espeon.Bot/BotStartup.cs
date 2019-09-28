using Casino.Common;
using Casino.DependencyInjection;
using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot
{
    public class BotStartup
    {
        private readonly IServiceProvider _services;

        [Inject] private readonly ICommandHandlingService _commands;
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IEventsService _events;

        private readonly Config _config;

        private readonly TaskCompletionSource<bool> _tcs;

        public BotStartup(IServiceProvider services, Config config)
        {
            _services = services;
            _config = config;

            _tcs = new TaskCompletionSource<bool>();
        }

        public async Task StartAsync(UserStore userStore, CommandStore commandStore)
        {
            EventHooks(userStore);

            await _commands.SetupCommandsAsync(commandStore);

            await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
            await _client.StartAsync();

            await _tcs.Task;
        }

        private void EventHooks(UserStore userStore)
        {
            var logger = _services.GetService<ILogService>();

            async Task ReadyAsync()
            {
                await _services.GetService<IReminderService>().LoadRemindersAsync(userStore);
                _ = Task.Run(() => _services.GetService<IStatusService>().RunStatusesAsync());

                _client.Ready -= ReadyAsync;
                _tcs.SetResult(true);

#if DEBUG
                Console.Beep(5000, 100);
#endif
            }

            _client.Ready += ReadyAsync;

            _client.UserJoined += user => _events.RegisterEvent(async () =>
            {
                using var guildStore = _services.GetService<GuildStore>();

                var dbGuild = await guildStore.GetOrCreateGuildAsync(user.Guild);
                var guild = user.Guild;

                if (guild.GetTextChannel(dbGuild.WelcomeChannelId) is { } channel
                    && !string.IsNullOrWhiteSpace(dbGuild.WelcomeMessage))
                {
                    var str = dbGuild.WelcomeMessage
                        .Replace("{{guild}}", user.Guild.Name)
                        .Replace("{{user}}", user.GetDisplayName());

                    await channel.SendMessageAsync(user.Mention, embed: new EmbedBuilder
                    {
                        Title = "A User Appears!",
                        Color = Utilities.EspeonColor,
                        Description = str,
                        ThumbnailUrl = user.GetAvatarOrDefaultUrl()
                    }
                        .Build());
                }

                if (guild.GetRole(dbGuild.DefaultRoleId) is { } role)
                {
                    await user.AddRoleAsync(role, new RequestOptions
                    {
                        AuditLogReason = "Auto role on join"
                    });
                }
            });

            _client.JoinedGuild += guild => _events.RegisterEvent(async () =>
            {
                var channelName = new[] { "welcome", "introduction", "general" };

                var channel = guild.TextChannels
                    .FirstOrDefault(
                        x => channelName.Any(y => x.Name.Contains(y, StringComparison.InvariantCultureIgnoreCase)))
                            ?? guild.TextChannels.FirstOrDefault(x => guild.CurrentUser.GetPermissions(x).ViewChannel
                                && guild.CurrentUser.GetPermissions(x).SendMessages);

                if (channel is null)
                    return;

                await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                {
                    Title = "",
                    Color = Utilities.EspeonColor,
                    ThumbnailUrl = guild.CurrentUser.GetAvatarOrDefaultUrl(),
                    Description = $"Hello! I am Espeon{_services.GetService<IEmoteService>().Collection["Espeon"]} " +
                    $"and I have just been added to your guild!\n" +
                    $"Type es/help to see all my available commands!"
                }
                    .Build());
            });

            _client.Log += log => _events.RegisterEvent(() =>
            {
                logger.Log(Source.Discord, (Severity)(int)log.Severity, log.Message, log.Exception);
                return Task.CompletedTask;
            });

            _services.GetService<TaskQueue>().OnError += ex => _events.RegisterEvent(() =>
            {
                logger.Log(Source.Scheduler, Severity.Error, string.Empty, ex);
                return Task.CompletedTask;
            });
        }
    }
}
