using Discord;
using Discord.WebSocket;
using Espeon.Attributes;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    public class EspeonStartup
    {
        private readonly IServiceProvider _services;

        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly CommandService _commands;

        private readonly Config _config;
        private bool _ran;

        public EspeonStartup(IServiceProvider services, Config config)
        {
            _services = services;
            _config = config;
            _ran = false;
        }

        public async Task StartBotAsync(UserStore userStore)
        {
            EventHooks(userStore);
            
            _commands.AddModules(Assembly.GetEntryAssembly());

            await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
            await _client.StartAsync();
        }

        //TODO clean this up
        //TODO events to remove stuff from DB e.g. role deleted, user leave
        private void EventHooks(UserStore userStore)
        {
            _client.Ready += async () =>
            {
                if(!_ran)
                {
                    await _services.GetService<ReminderService>().LoadRemindersAsync(userStore);
                    _ran = true;
                }
            };

            _client.UserJoined += async user =>
            {
                using var guildStore = _services.GetService<GuildStore>();

                var dbGuild = await guildStore.GetOrCreateGuildAsync(user.Guild);
                var guild = user.Guild;

                if (guild.GetTextChannel(dbGuild.WelcomeChannelId) is SocketTextChannel channel 
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

                if(guild.GetRole(dbGuild.DefaultRoleId) is SocketRole role)
                {
                    await user.AddRoleAsync(role);
                }
            };

            _client.JoinedGuild += async guild =>
            {
                var channelName = new[] { "welcome", "introduction", "general" };

                var channel = guild.TextChannels
                    .FirstOrDefault(x => channelName.Any(y => x.Name.Contains(y, StringComparison.InvariantCultureIgnoreCase)))
                        ?? guild.TextChannels.FirstOrDefault(x => guild.CurrentUser.GetPermissions(x).ViewChannel
                            && guild.CurrentUser.GetPermissions(x).SendMessages);

                if (channel is null)
                    return;

                await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                {
                    Title = "",
                    Color = Utilities.EspeonColor,
                    ThumbnailUrl = guild.CurrentUser.GetAvatarOrDefaultUrl(),
                    Description = $"Hello! I am Espeon{_services.GetService<EmotesService>().Collection["Espeon"]} and I have just been added to your guild!\n" +
                    $"Type es/help to see all my available commands!"
                }
                    .Build());
            };

            var logger = _services.GetService<LogService>();
            _client.Log += log =>
            {
                return logger.LogAsync(Source.Discord, (Severity)(int)log.Severity, log.Message, log.Exception);
            };
        }
    }
}
