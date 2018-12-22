using Espeon.Commands.Checks;
using Espeon.Core;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using Espeon.Entities;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(typeof(ICustomCommandsService), ServiceLifetime.Singleton, true)]
    public class CustomCommandsService : ICustomCommandsService
    {
        [Inject] private readonly IDatabaseService _database;
        [Inject] private readonly ILogService _log;
        [Inject] private readonly IMessageService _massage;
        [Inject] private readonly CommandService _commands;

        private readonly ConcurrentDictionary<ulong, Module> _moduleCache;

        public CustomCommandsService()
        {
            _moduleCache = new ConcurrentDictionary<ulong, Module>();
        }

        [Initialiser]
        private async Task InitialiseAsync()
        {
            var guilds = await _database.GetCollectionAsync<Guild>("guilds");

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
            if (!(originaContext is IEspeonContext context))
                throw new ExpectedContextException("IEspeonContext");


            var guild = await _database.GetEntityAsync<Guild>("guilds", context.Guild.Id);
            var commands = guild.Data.Commands;
            var found = commands.First(x => string.Equals(x.Name, command.Name));

            await _massage.SendMessageAsync(context, found.Value, null);

            return new SuccessfulResult();
        }

        public async Task<bool> TryCreateCommandAsync(IEspeonContext context, string name, string value)
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

        public Task<bool> TryDeleteCommandAsync(IEspeonContext context, BaseCustomCommand command)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryModifyCommandAsync(IEspeonContext context, BaseCustomCommand command, string newValue)
        {
            throw new System.NotImplementedException();
        }

        public Task<ImmutableArray<BaseCustomCommand>> GetCommandsAsync(ulong id)
        {
            throw new System.NotImplementedException();
        }
    }
}
