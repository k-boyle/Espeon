﻿using Casino.Common.DependencyInjection;
using Espeon.Commands;
using Espeon.Databases;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class CustomCommandsService : BaseService<InitialiseArgs>
    {
        [Inject] private readonly LogService _log;
        [Inject] private readonly MessageService _message;
        [Inject] private readonly CommandService _commands;

        private readonly ConcurrentDictionary<ulong, Module> _moduleCache;

        public CustomCommandsService(IServiceProvider services) : base(services)
        {
            _moduleCache = new ConcurrentDictionary<ulong, Module>();
        }

        public override async Task InitialiseAsync(IServiceProvider services, InitialiseArgs args)
        {
            var guilds = await args.GuildStore.GetAllGuildsAsync(x => x.Commands);

            var createCommands = guilds.Select(CreateCommandsAsync);

            await Task.WhenAll(createCommands);
            _log.Log(Source.Commands, Severity.Verbose, "All custom commands loaded");
        }

        private Task CreateCommandsAsync(Guild guild)
        {
            var commands = guild.Commands;

            if (commands is null || commands.Count == 0)
                return Task.CompletedTask;

            var module = _commands.AddModule(moduleBuilder =>
            {
                moduleBuilder.Name = guild.Id.ToString();
                moduleBuilder.AddCheck(new RequireGuildAttribute(guild.Id));

                foreach (var command in commands)
                {
                    if (command.Name is null)
                        continue;

                    moduleBuilder.AddCommand(CommandCallbackAsync, commandBuilder =>
                    {
                        commandBuilder.Name = command.Name;
                        commandBuilder.AddAliases(command.Name);
                    });
                }
            });

            _moduleCache[guild.Id] = module;

            return Task.CompletedTask;
        }

        private async ValueTask<IResult> CommandCallbackAsync(CommandContext originalContext, IServiceProvider services)
        {
            var context = (EspeonContext)originalContext;

            var guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
            var commands = guild?.Commands;

            var found = commands?.FirstOrDefault(x =>
                string.Equals(x.Name, context.Command.Name, StringComparison.InvariantCultureIgnoreCase));

            await _message.SendAsync(context, x => x.Content = found.Value);

            return new SuccessfulResult();
        }

        public async Task<bool> TryCreateCommandAsync(EspeonContext context, string name, string value)
        {
            if (_moduleCache.TryGetValue(context.Guild.Id, out var found))
            {
                var commands = found.Commands;

                if (!Utilities.AvailableName(commands, name))
                    return false;
            }

            var loadedCommands = _commands.GetAllCommands();
            var filtered = loadedCommands.Where(x => !ulong.TryParse(x.Module.Name, out _));

            if (!Utilities.AvailableName(filtered, name))
                return false;

            var newCmd = new CustomCommand
            {
                Name = name,
                Value = value
            };

            var guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
            guild.Commands.Add(newCmd);
            context.GuildStore.Update(guild);

            await context.GuildStore.SaveChangesAsync();
            await UpdateCommandsAsync(guild);

            return true;
        }

        public async Task DeleteCommandAsync(EspeonContext context, CustomCommand command)
        {
            var guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
            guild.Commands.Remove(command);
            context.GuildStore.Update(guild);

            await context.GuildStore.SaveChangesAsync();

            await UpdateCommandsAsync(guild);
        }

        public Task ModifyCommandAsync(EspeonContext context, CustomCommand command, string newValue)
        {
            command.Value = newValue;
            context.GuildStore.Update(command);

            return context.GuildStore.SaveChangesAsync();
        }

        public async Task<ImmutableArray<CustomCommand>> GetCommandsAsync(EspeonContext context)
        {
            var guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
            return guild.Commands.ToImmutableArray();
        }

        private async Task UpdateCommandsAsync(Guild guild)
        {
            if(_moduleCache.ContainsKey(guild.Id))
                _commands.RemoveModule(_moduleCache[guild.Id]);

            await CreateCommandsAsync(guild);
        }
    }
}
