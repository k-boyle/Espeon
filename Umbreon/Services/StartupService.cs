using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Umbreon.Helpers;
using Discord;

namespace Umbreon.Services
{
    public class StartupService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseService _database;
        private readonly EventsService _events;
        private readonly IServiceProvider _services;

        public StartupService(DiscordSocketClient client, CommandService commands, DatabaseService database, EventsService events, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _database = database;
            _events = events;
            _services = services;
        }

        public async Task InitialiseAsync()
        {
            await _database.Initialize();
            await StartClient();
            _events.HookEvents();
            await LoadCommands();
            await Task.Delay(-1);
        }

        private async Task StartClient()
        {
            await _client.LoginAsync(TokenType.Bot, ConstantsHelper.BotToken);
            await _client.StartAsync();
        }

        private async Task LoadCommands()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
