using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace Umbreon.Services
{
    public class EventsService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private readonly LogService _logs;
        private readonly CommandHandler _handler;

        public EventsService(DiscordSocketClient client, CommandService commands, DatabaseService database, LogService logs, CommandHandler handler)
        {
            _client = client;
            _commands = commands;
            _database = database;
            _logs = logs;
            _handler = handler;
        }

        public void HookEvents()
        {
            _client.Ready += ClientReady;
            _client.Log += _logs.LogEvent;
            _commands.Log +=_logs.LogEvent;
            _client.MessageReceived += _handler.HandleMessageAsync;
            _client.MessageUpdated += MessageUpdated;
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            await _handler.HandleMessageAsync(arg2);
        }

        private Task ClientReady()
        {
            _database.LoadGuilds();
            return Task.CompletedTask;
        }
    }
}
