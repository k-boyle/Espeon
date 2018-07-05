using Discord;
using Discord.Commands;
using Discord.Net.Helpers;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Umbreon.Services
{
    public class EventsService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private readonly LogService _logs;
        private readonly CommandHandler _handler;
        private readonly CustomCommandsService _customCommands;

        public EventsService(DiscordSocketClient client, CommandService commands, DatabaseService database, LogService logs, CommandHandler handler, CustomCommandsService customCommands)
        {
            _client = client;
            _commands = commands;
            _database = database;
            _logs = logs;
            _handler = handler;
            _customCommands = customCommands;
        }

        public void HookEvents()
        {
            _client.Ready += ClientReady;
            _client.Log += _logs.LogEvent;
            _client.MessageReceived += _handler.HandleMessageAsync;
            _client.MessageUpdated += MessageUpdated;
            _client.JoinedGuild += async guild =>
            {
                _database.NewGuild(guild);
                var channel = guild.GetDefaultChannel(guild.CurrentUser);
                if (!(channel is null))
                {
                    await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                    {
                        // TODO THIS
                    }.Build());
                }
            };
            _commands.Log += _logs.LogEvent;
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            await _handler.HandleMessageAsync(arg2);
        }

        private async Task ClientReady()
        {
            _database.LoadGuilds();
            await _customCommands.LoadCmds(_client);
        }
    }
}
