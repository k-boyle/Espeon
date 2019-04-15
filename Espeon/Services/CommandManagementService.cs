using Espeon.Commands;
using Espeon.Databases;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class CommandManagementService : BaseService
    {
        [Inject] private readonly CommandService _commands;

        public CommandManagementService(IServiceProvider services) : base(services)
        {
        }

        public async Task<bool> AddAliasAsync(EspeonContext context, Module module, string alias)
        {
            var commands = _commands.GetAllCommands();

            if (!Utilities.AvailableName(commands, alias))
            {
                return false;
            }

            var foundModule = await context.CommandStore.Modules.FindAsync(module.Name);

            if (foundModule is null)
                return false;

            if (foundModule.Aliases is null)
                foundModule.Aliases = new List<string>();

            foundModule.Aliases.Add(alias);
            context.CommandStore.Update(foundModule);

            await context.CommandStore.SaveChangesAsync();
            Update(module, foundModule);

            return true;
        }

        public async Task<bool> AddAliasAsync(EspeonContext context, Module module, string command, string alias)
        {
            var commands = _commands.GetAllCommands();

            if (!Utilities.AvailableName(commands, alias))
                return false;

            var foundModule = await context.CommandStore.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == module.Name);

            var foundCommand = foundModule?.Commands.SingleOrDefault(x => x.Name == command);

            if (foundCommand is null)
                return false;

            if (foundCommand.Aliases is null)
                foundCommand.Aliases = new List<string>();

            foundCommand.Aliases.Add(alias);
            context.CommandStore.Update(foundModule);

            await context.CommandStore.SaveChangesAsync();
            Update(module, foundModule);

            return true;
        }

        public async Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string alias)
        {
            var foundModule = await context.CommandStore.Modules.FindAsync(module.Name);

            if (foundModule?.Aliases is null || !foundModule.Aliases.Contains(alias))
                return false;

            foundModule.Aliases.Remove(alias);
            context.CommandStore.Update(foundModule);

            await context.CommandStore.SaveChangesAsync();
            Update(module, foundModule);

            return true;
        }

        public async Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string command, string alias)
        {
            var foundModule = await context.CommandStore.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == module.Name);

            var foundCommand = foundModule?.Commands.SingleOrDefault(x => x.Name == command);

            if (foundCommand?.Aliases is null || !foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Remove(alias);
            context.CommandStore.Update(foundModule);

            await context.CommandStore.SaveChangesAsync();
            Update(module, foundModule);

            return true;
        }

        private void Update(Module module, ModuleInfo info)
        {
            _commands.RemoveModule(module);
            _commands.AddModule(module.Type, 
                builder =>
                {
                    if (!(info.Aliases is null) && info.Aliases.Count > 0)
                        builder.AddAliases(info.Aliases.ToArray());

                    foreach(var command in builder.Commands)
                    {
                        var foundCommand = info.Commands.FirstOrDefault(x => x.Name == command.Name);

                        if (!(foundCommand.Aliases is null) && foundCommand.Aliases.Count > 0)
                            command.AddAliases(foundCommand.Aliases.ToArray());
                    }
                });
        }
    }
}
