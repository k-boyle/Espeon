using Espeon.Attributes;
using Espeon.Commands;
using Espeon.Commands.Checks;
using Espeon.Database;
using Espeon.Database.Entities;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class CustomCommandsService : IService
    {
        [Inject] private readonly LogService _log;
        [Inject] private readonly MessageService _massage;
        [Inject] private readonly CommandService _commands;

        private readonly ConcurrentDictionary<ulong, Module> _moduleCache;

        public CustomCommandsService()
        {
            _moduleCache = new ConcurrentDictionary<ulong, Module>();
        }

        public async Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
        {
            var guilds = context.Guilds;

            var createCommands = guilds.Select(CreateCommandsAsync);

            await Task.WhenAll(createCommands);
            await _log.LogAsync(Source.Commands, Severity.Verbose, "All custom commands loaded");
        }

        private async Task CreateCommandsAsync(Guild guild)
        {
            var commands = guild.Data.Commands;

            if (commands.Count == 0)
                return;

            var module = await _commands.AddModuleAsync(moduleBuilder =>
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
        }

        private async Task<IResult> CommandCallbackAsync(Command command, object[] parameters,
            ICommandContext originaContext, IServiceProvider services)
        {
            if (!(originaContext is EspeonContext context))
                throw new ExpectedContextException("IEspeonContext");


            var guild = await context.Database.Guilds.FindAsync(context.Guild.Id);
            var commands = guild.Data.Commands;
            var found = commands.First(x => string.Equals(x.Name, command.Name));

            await _massage.SendMessageAsync(context, found.Value);

            return new SuccessfulResult();
        }

        public async Task<bool> TryCreateCommandAsync(EspeonContext context, string name, string value)
        {
            if (_moduleCache.TryGetValue(context.Guild.Id, out var found))
            {
                var commands = found.Commands;

                if (commands.Any(x => x.FullAliases
                    .Any(y => string.Equals(y, name, StringComparison.InvariantCultureIgnoreCase))))
                    return false;
            }

            return true;
        }

        public Task<bool> TryDeleteCommandAsync(EspeonContext context, CustomCommand command)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryModifyCommandAsync(EspeonContext context, CustomCommand command, string newValue)
        {
            throw new System.NotImplementedException();
        }

        public Task<ImmutableArray<CustomCommand>> GetCommandsAsync(ulong id)
        {
            throw new System.NotImplementedException();
        }
    }
}
