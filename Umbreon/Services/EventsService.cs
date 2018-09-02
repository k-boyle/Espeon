using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using Umbreon.Attributes;
using Umbreon.Core.Entities.Guild;
using Umbreon.Extensions;

namespace Umbreon.Services
{
    [Service]
    public class EventsService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private readonly LogService _logs;
        private readonly CustomCommandsService _customCommands;
        private readonly CustomFunctionService _customFunctions;
        private readonly MusicService _musicService;
        private readonly MessageService _message;
        private readonly RemindersService _reminders;

        public EventsService(DiscordSocketClient client, CommandService commands, DatabaseService database, LogService logs, 
            CustomCommandsService customCommands, CustomFunctionService customFunctions, MusicService musicService, 
            MessageService message, RemindersService reminders)
        {
            _client = client;
            _commands = commands;
            _database = database;
            _logs = logs;
            _customCommands = customCommands;
            _customFunctions = customFunctions;
            _musicService = musicService;
            _message = message;
            _reminders = reminders;
        }

        public void HookEvents()
        {
            _client.Log += _logs.LogEvent;
            _client.Ready += async () =>
            {
                await _customCommands.LoadCmdsAsync(_client);
                await _customFunctions.LoadFuncsAsync(_client);
                //await _musicService.InitialiseAsync();
                await _reminders.LoadRemindersAsync();
            };
            _client.MessageReceived += _message.HandleMessageAsync;
            _client.MessageUpdated += (_, msg, __) => _message.HandleMessageUpdateAsync(msg);
            _client.JoinedGuild += async guild =>
            {
                var channel = guild.GetDefaultChannel();
                if (!(channel is null))
                {
                    await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = _client.CurrentUser.GetAvatarOrDefaultUrl(),
                            Name = guild.CurrentUser.GetDisplayName()
                        },
                        Color = new Color(0, 0, 0),
                        ThumbnailUrl = _client.CurrentUser.GetDefaultAvatarUrl(),
                        Description = $"Hello! I am {guild.CurrentUser.GetDisplayName()} and I have just been added to your guild!\n" +
                                      $"Type {_database.GetObject<GuildObject>("guilds", guild.Id).Prefixes.First()}help to see all my available commands!"
                    }.Build());
                }
            };
            _commands.Log += _logs.LogEvent;
            _commands.CommandExecuted += _message.CommandExecutedAsync;
        }
    }
}
