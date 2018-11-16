using System;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using Espeon.Entities;
using Qmmands;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(typeof(ICustomCommandsService), true)]
    public class CustomCommandsService : ICustomCommandsService
    {
        [Inject] private readonly IDatabaseService _database;
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

            foreach (var guild in guilds)
            {
                var commands = guild.Data.Commands;

                if(commands.Count == 0)
                    continue;

                var module = await _commands.AddModuleAsync(moduleBuilder =>
                    {
                        moduleBuilder.Name = guild.Id.ToString();
                        //TODO add require guild check

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
            }
        }

        private async Task<IResult> CommandCallbackAsync(Command command, object[] parameters, ICommandContext originaContext, IServiceProvider services)
        {
            if(!(originaContext is IEspeonContext context))
                return new EspeonResult(false, "Expected IEspeonContext");

            return new EspeonResult(true, null);
        }

        public Task<bool> TryCreateCommandAsync(IEspeonContext context, string name, string value)
        {
            throw new System.NotImplementedException();
        }

        public Task TryDeleteCommandAsync(IEspeonContext context, BaseCustomCommand command)
        {
            throw new System.NotImplementedException();
        }

        public Task TryModifyCommandAsync(IEspeonContext context, BaseCustomCommand command, string newValue)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<BaseCustomCommand>> GetCommandsAsync(ulong id)
        {
            throw new System.NotImplementedException();
        }
    }
}
