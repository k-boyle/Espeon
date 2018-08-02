using Discord;
using Discord.Commands;
using Discord.Net.Helpers;
using Discord.WebSocket;
using System.Linq;
using Umbreon.Attributes;

namespace Umbreon.Services
{
    [Service]
    public class EventsService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private readonly LogService _logs;
        private readonly CommandHandler _handler;
        private readonly CustomCommandsService _customCommands;
        private readonly CustomFunctionService _customFunctions;
        private readonly MusicService _musicService;

        public EventsService(DiscordSocketClient client, CommandService commands, DatabaseService database, LogService logs, 
            CommandHandler handler, CustomCommandsService customCommands, CustomFunctionService customFunctions, MusicService musicService)
        {
            _client = client;
            _commands = commands;
            _database = database;
            _logs = logs;
            _handler = handler;
            _customCommands = customCommands;
            _customFunctions = customFunctions;
            _musicService = musicService;
        }

        public void HookEvents()
        {
            _client.Log += _logs.LogEvent;
            _client.Ready += async () =>
            {
                _database.LoadGuilds();
                await _customCommands.LoadCmds(_client);
                await _customFunctions.LoadFuncs(_client);
                await _musicService.Initialise();
            };
            _client.MessageReceived += _handler.HandleMessageAsync;
            _client.MessageUpdated += async (_, message, __) => { await _handler.HandleMessageAsync(message); };
            _client.JoinedGuild += async guild =>
            {
                _database.NewGuild(guild);
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
                                      $"Type {_database.GetGuild(guild.Id).Prefixes.First()}help to see all my available commands!"
                    }.Build());
                }
            };
            _commands.Log += _logs.LogEvent;
        }
    }
}
