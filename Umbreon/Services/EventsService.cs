using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Attributes;
using Umbreon.Core.Entities.Guild;
using Umbreon.Extensions;

namespace Umbreon.Services
{
    [Service]
    public class EventsService
    {
        private readonly IServiceProvider _services;

        public EventsService(IServiceProvider services)
        {
            _services = services;
        }

        public void HookEvents()
        {
            var client = _services.GetService<DiscordSocketClient>();
            var logs = _services.GetService<LogService>();
            var message = _services.GetService<MessageService>();
            var commands = _services.GetService<CommandService>();
            client.Log += logs.LogEvent;
            client.Ready += async () =>
            {
                await _services.GetService<CustomCommandsService>().LoadCmdsAsync(client);
                //await _musicService.InitialiseAsync();
                await _services.GetService<RemindersService>().LoadRemindersAsync();
            };
            client.MessageReceived += message.HandleMessageAsync;
            client.MessageUpdated += (_, msg, __) => message.HandleMessageUpdateAsync(msg);
            client.JoinedGuild += async guild =>
            {
                var channel = guild.GetDefaultChannel();
                if (!(channel is null))
                {
                    await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = client.CurrentUser.GetAvatarOrDefaultUrl(),
                            Name = guild.CurrentUser.GetDisplayName()
                        },
                        Color = new Color(0, 0, 0),
                        ThumbnailUrl = client.CurrentUser.GetDefaultAvatarUrl(),
                        Description = $"Hello! I am {guild.CurrentUser.GetDisplayName()} and I have just been added to your guild!\n" +
                                      $"Type {_services.GetService<DatabaseService>().GetObject<GuildObject>("guilds", guild.Id).Prefixes.First()}help to see all my available commands!"
                    }.Build());
                }
            };
            commands.Log += logs.LogEvent;
        }
    }
}
