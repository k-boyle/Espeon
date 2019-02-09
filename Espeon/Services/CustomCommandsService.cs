using Espeon.Attributes;
using Espeon.Commands;
using Espeon.Commands.Checks;
using Espeon.Database;
using Espeon.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class CustomCommandsService : BaseService
    {
        [Inject] private readonly LogService _log;
        [Inject] private readonly MessageService _message;
        [Inject] private readonly CommandService _commands;

        private readonly ConcurrentDictionary<ulong, Module> _moduleCache;

        public CustomCommandsService()
        {
            _moduleCache = new ConcurrentDictionary<ulong, Module>();
        }

        public override async Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
        {
            var guilds = context.Guilds.Include(x => x.Commands);

            var createCommands = guilds.Select(CreateCommandsAsync);

            await Task.WhenAll(createCommands);
            await _log.LogAsync(Source.Commands, Severity.Verbose, "All custom commands loaded");
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
                    moduleBuilder.AddCommand(commandBuilder =>
                    {
                        commandBuilder.Name = command.Name;
                        commandBuilder.AddAliases(command.Name);
                        commandBuilder.WithCallback(CommandCallbackAsync);
                    });
                }
            });

            _moduleCache[guild.Id] = module;

            return Task.CompletedTask;
        }

        private async Task<IResult> CommandCallbackAsync(Command command, object[] parameters,
            ICommandContext originalContext, IServiceProvider services)
        {
            var context = originalContext as EspeonContext;

            var guild = await context!.GetCurrentGuildAsync(x => x.Commands);
            var commands = guild?.Commands;

            var found = commands?.FirstOrDefault(x =>
                string.Equals(x.Name, command.Name, StringComparison.InvariantCultureIgnoreCase));

            await _message.SendMessageAsync(context, found!.Value);

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

            if (Utilities.AvailableName(filtered, name))
                return false;

            var newCmd = new CustomCommand
            {
                Name = name,
                Value = value,
                GuildId = context.Guild.Id
            };

            await context.Database.CustomCommands.AddAsync(newCmd);
            await context.Database.SaveChangesAsync();

            var guild = await context.Database.Guilds.FindAsync(context.Guild.Id);

            await UpdateCommandsAsync(guild);

            return true;
        }

        public async Task DeleteCommandAsync(EspeonContext context, CustomCommand command)
        {
            context.Database.CustomCommands.Remove(command);
            await context.Database.SaveChangesAsync();

            var guild = await context.Database.Guilds.FindAsync(context.Guild.Id);

            await UpdateCommandsAsync(guild);
        }

        public Task ModifyCommandAsync(EspeonContext context, CustomCommand command, string newValue)
        {
            command.Value = newValue;

            context.Database.CustomCommands.Update(command);

            return context.Database.SaveChangesAsync();
        }

        public async Task<ImmutableArray<CustomCommand>> GetCommandsAsync(EspeonContext context)
        {
            var guild = await context.GetCurrentGuildAsync(x => x.Commands);
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
