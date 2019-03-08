using Espeon.Commands;
using Espeon.Databases;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class CommandsManager : BaseService
    {
        [Inject] private readonly CommandService _commands;

        //TODO doesn't work
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

            foundModule.Aliases.Add(alias);
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

            var foundCommand = foundModule?.Commands.SingleOrDefault(x => x.Name == $"{module.Name}{command}");

            if (foundCommand is null || foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Add(alias);

            await context.CommandStore.SaveChangesAsync();
            Update(module, foundModule);

            return true;
        }

        public async Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string alias)
        {
            var foundModule = await context.CommandStore.Modules.FindAsync(module.Name);

            if (foundModule is null || !foundModule.Aliases.Contains(alias))
                return false;

            foundModule.Aliases.Remove(alias);

            await context.CommandStore.SaveChangesAsync();
            Update(module, foundModule);

            return true;
        }

        public async Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string command, string alias)
        {
            var foundModule = await context.CommandStore.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == module.Name);

            var foundCommand = foundModule?.Commands.SingleOrDefault(x => x.Name == $"{module.Name}{command}");

            if (foundCommand is null || !foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Remove(alias);

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
