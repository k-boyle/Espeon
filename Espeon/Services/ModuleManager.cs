using Espeon.Core.Attributes;
using Espeon.Core.Services;
using Espeon.Entities;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(typeof(IModuleManager), true)]
    public class ModuleManager : IModuleManager
    {
        [Inject] private readonly IDatabaseService _database;
        [Inject] private readonly CommandService _commands;

        public async Task OnBuildingAsync(ModuleBuilder moduleBuilder)
        {
            var modules = await _database.GetCollectionAsync<ModuleInfo>("modules");
            var foundModule = modules.FirstOrDefault(x => x.Name == moduleBuilder.Name);

            if (foundModule is null)
            {
                foundModule = new ModuleInfo
                {
                    Name = moduleBuilder.Name,
                    Commands = moduleBuilder.Commands.Select(x => new CommandInfo
                    {
                        Name = x.Name
                    }).ToArray()
                };

                await _database.WriteAsync("modules", foundModule);
                return;
            }

            moduleBuilder.AddAliases(foundModule.Aliases);

            foreach (var commandBuilder in moduleBuilder.Commands)
            {
                var foundCommand = foundModule.Commands.FirstOrDefault(x => x.Name == commandBuilder.Name);

                if (foundCommand is null)
                {
                    foundCommand = new CommandInfo
                    {
                        Name = commandBuilder.Name
                    };

                    foundModule.Commands.Add(foundCommand);

                    continue;
                }

                commandBuilder.AddAliases(foundCommand.Aliases);
            }
        }

        public async Task<bool> AddAliasAsync(Module module, string alias)
        {
            var modules = await _database.GetCollectionAsync<ModuleInfo>("modules");
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            if (foundModule.Aliases.Contains(alias))
                return false;

            foundModule.Aliases.Add(alias);

            await _database.WriteAsync("modules", foundModule);
            await UpdateAsync(module);
            return true;
        }

        public async Task<bool> AddAliasAsync(Module module, string command, string alias)
        {
            var modules = await _database.GetCollectionAsync<ModuleInfo>("modules");
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            var foundCommand = foundModule.Commands.Single(x => x.Name == command);

            if (foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Add(alias);

            await _database.WriteAsync("modules", foundModule);
            await UpdateAsync(module);
            return true;
        }

        public async Task<bool> RemoveAliasAsync(Module module, string alias)
        {
            var modules = await _database.GetCollectionAsync<ModuleInfo>("modules");
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            if (!foundModule.Aliases.Contains(alias))
                return false;

            foundModule.Aliases.Remove(alias);

            await _database.WriteAsync("modules", foundModule);
            await UpdateAsync(module);
            return true;
        }

        public async Task<bool> RemoveAliasAsync(Module module, string command, string alias)
        {
            var modules = await _database.GetCollectionAsync<ModuleInfo>("modules");
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            var foundCommand = foundModule.Commands.Single(x => x.Name == command);

            if (!foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Remove(alias);

            await _database.WriteAsync("modules", foundModule);
            await UpdateAsync(module);
            return true;
        }

        private async Task UpdateAsync(Module module)
        {
            await _commands.RemoveModuleAsync(module);
            await _commands.AddModuleAsync(module.Type);
        }
    }
}
