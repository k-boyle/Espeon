using Espeon.Attributes;
using Espeon.Entities;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(ServiceLifetime.Singleton)]
    public class ModuleManager
    {
        [Inject] private readonly DatabaseService _database;
        [Inject] private readonly CommandService _commands;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());
        
        [Initialiser]
        public void Initialiser()
        {
            _commands.ModuleBuilding += OnBuildingAsync;
        }

        private async Task OnBuildingAsync(ModuleBuilder moduleBuilder)
        {
            if(string.IsNullOrWhiteSpace(moduleBuilder.Name))
                throw new ArgumentNullException(nameof(moduleBuilder.Name));

            var modules = await _database.GetCollectionAsync<ModuleInfo>("modules");
            var foundModule = modules.FirstOrDefault(x => x.Name == moduleBuilder.Name);

            if (foundModule is null)
            {
                foundModule = new ModuleInfo
                {
                    Id = (ulong) Random.Next(),
                    Name = moduleBuilder.Name
                };

                var list = new List<CommandInfo>();

                foreach (var commandBuilder in moduleBuilder.Commands)
                {
                    if(string.IsNullOrWhiteSpace(commandBuilder.Name))
                        throw new ArgumentNullException(nameof(commandBuilder.Name));

                    list.Add(new CommandInfo
                    {
                        Name = commandBuilder.Name
                    });
                }

                await _database.WriteEntityAsync("modules", foundModule);
                return;
            }

            moduleBuilder.AddAliases(foundModule.Aliases.ToArray());

            foreach (var commandBuilder in moduleBuilder.Commands)
            {
                if (string.IsNullOrWhiteSpace(commandBuilder.Name))
                    throw new ArgumentNullException(nameof(commandBuilder.Name));

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

                commandBuilder.AddAliases(foundCommand.Aliases.ToArray());
            }
        }

        public async Task<bool> AddAliasAsync(Module module, string alias)
        {
            var modules = await _database.GetCollectionAsync<ModuleInfo>("modules");
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            if (foundModule.Aliases.Contains(alias))
                return false;

            foundModule.Aliases.Add(alias);

            await _database.WriteEntityAsync("modules", foundModule);
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

            await _database.WriteEntityAsync("modules", foundModule);
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

            await _database.WriteEntityAsync("modules", foundModule);
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

            await _database.WriteEntityAsync("modules", foundModule);
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
