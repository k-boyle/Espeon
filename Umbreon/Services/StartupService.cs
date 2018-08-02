using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core.Models.Database;
using Umbreon.Core.Models.Database.Guilds;
using Umbreon.Helpers;
using Umbreon.TypeReaders;

namespace Umbreon.Services
{
    [Service]
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
            _commands.AddTypeReader(typeof(ModuleInfo), new ModuleInfoTypeReader());
            _commands.AddTypeReader(typeof(IEnumerable<CommandInfo>), new CommandInfoTypeReader());
            _commands.AddTypeReader(typeof(Tag), new TagTypeReader());
            _commands.AddTypeReader(typeof(CustomCommand), new CustomCommandTypeReader());
            _commands.AddTypeReader(typeof(CustomFunction), new CustomFunctionTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
