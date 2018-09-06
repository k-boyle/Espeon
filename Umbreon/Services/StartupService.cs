using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Umbreon.Attributes;
using Umbreon.Commands.TypeReaders;
using Umbreon.Core.Entities.Guild;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Helpers;

namespace Umbreon.Services
{
    [Service]
    public class StartupService
    {
        private readonly IServiceProvider _services;

        public StartupService(IServiceProvider services)
        {
            _services = services;
        }

        public async Task InitialiseAsync()
        {
            await DatabaseService.Initialise();
            _services.GetService<PokemonDataService>().Initialise();
            _services.GetService<PokemonPlayerService>().Initialise();
            await StartClientAsync();
            _services.GetService<EventsService>().HookEvents();
            _services.GetRequiredService<TimerService>().InitialiseTimer();
            await LoadCommandsAsync();
            await Task.Delay(-1);
        }

        private async Task StartClientAsync()
        {
            var client = _services.GetService<DiscordSocketClient>();
            await client.LoginAsync(TokenType.Bot, ConstantsHelper.BotToken);
            await client.StartAsync();
        }

        private async Task LoadCommandsAsync()
        {
            var commands = _services.GetService<CommandService>();
            commands.AddTypeReader(typeof(ModuleInfo), new ModuleInfoTypeReader());
            commands.AddTypeReader(typeof(IEnumerable<CommandInfo>), new CommandInfoTypeReader());
            commands.AddTypeReader(typeof(CustomCommand), new CustomCommandTypeReader());
            commands.AddTypeReader(typeof(PokemonData), new PokemonTypeReader());
            commands.AddTypeReader(typeof(Habitat), new HabitatTypeReader());
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
