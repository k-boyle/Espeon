using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Umbreon.Attributes;
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
        private readonly EventsService _events;
        private readonly TimerService _timer;
        private readonly IServiceProvider _services;

        public StartupService(DiscordSocketClient client, CommandService commands, EventsService events, TimerService timer, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _events = events;
            _timer = timer;
            _services = services;
        }

        public async Task InitialiseAsync()
        {
            await DatabaseService.Initialize();
            await StartClientAsync();
            _events.HookEvents();
            _timer.InitialiseTimer();
            await LoadCommandsAsync();
            await Task.Delay(-1);
        }

        private async Task StartClientAsync()
        {
            await _client.LoginAsync(TokenType.Bot, ConstantsHelper.BotToken);
            await _client.StartAsync();
        }

        private async Task LoadCommandsAsync()
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
