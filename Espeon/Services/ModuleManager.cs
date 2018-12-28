using Espeon.Attributes;
using Espeon.Commands;
using Espeon.Database;
using Espeon.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class ModuleManager : IService
    {
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly IServiceProvider _services;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        public Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
        {
            services.GetService<CommandService>().ModuleBuilding += OnBuildingAsync;
            return Task.CompletedTask;
        }
        
        private async Task OnBuildingAsync(ModuleBuilder moduleBuilder)
        {
            if (string.IsNullOrWhiteSpace(moduleBuilder.Name))
                throw new ArgumentNullException(nameof(moduleBuilder.Name));

            ModuleInfo foundModule;

            using (var scope = _services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetService<DatabaseContext>();

                var modules = ctx.Modules;
                foundModule = modules.FirstOrDefault(x => x.Name == moduleBuilder.Name);

                if (foundModule is null)
                {
                    foundModule = new ModuleInfo
                    {
                        Id = (ulong)Random.Next(),
                        Name = moduleBuilder.Name
                    };

                    var list = new List<CommandInfo>();

                    foreach (var commandBuilder in moduleBuilder.Commands)
                    {
                        if (string.IsNullOrWhiteSpace(commandBuilder.Name))
                            throw new ArgumentNullException(nameof(commandBuilder.Name));

                        list.Add(new CommandInfo
                        {
                            Name = commandBuilder.Name
                        });
                    }

                    foundModule.Commands = list;

                    await ctx.Modules.UpsertAsync(foundModule);
                    await ctx.SaveChangesAsync();
                }
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

        public async Task<bool> AddAliasAsync(EspeonContext context, Module module, string alias)
        {
            var modules = context.Database.Modules;
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            if (foundModule.Aliases.Contains(alias))
                return false;

            foundModule.Aliases.Add(alias);

            await context.Database.Modules.UpsertAsync(foundModule);
            await UpdateAsync(module);
            return true;
        }

        public async Task<bool> AddAliasAsync(EspeonContext context, Module module, string command, string alias)
        {
            var modules = context.Database.Modules;
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            var foundCommand = foundModule.Commands.Single(x => x.Name == command);

            if (foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Add(alias);

            context.Database.Commands.Update(foundCommand);
            await UpdateAsync(module);
            return true;
        }

        public async Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string alias)
        {
            var modules = context.Database.Modules;
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            if (!foundModule.Aliases.Contains(alias))
                return false;

            foundModule.Aliases.Remove(alias);

            await context.Database.Modules.UpsertAsync(foundModule);
            await UpdateAsync(module);
            return true;
        }

        public async Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string command, string alias)
        {
            var modules = context.Database.Modules;
            var foundModule = modules.FirstOrDefault(x => x.Name == module.Name);

            var foundCommand = foundModule.Commands.Single(x => x.Name == command);

            if (!foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Remove(alias);

            context.Database.Commands.Update(foundCommand);
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
