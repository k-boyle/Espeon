using Casino.DependencyInjection;
using Casino.Qmmands;
using Espeon.Core;
using Espeon.Core.Commands;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class CommandManagementService : BaseService<InitialiseArgs>, ICommandManagementService {
		[Inject] private readonly CommandService _commands;

		public CommandManagementService(IServiceProvider services) : base(services) { }

		async Task<bool> ICommandManagementService.AddAliasAsync(EspeonContext context, Module module, string alias) {
			IReadOnlyList<Command> commands = this._commands.GetAllCommands();

			if (!Utilities.AvailableName(commands, alias)) {
				return false;
			}

			ModuleInfo foundModule = await context.CommandStore.Modules.Include(x => x.Commands)
				.FirstOrDefaultAsync(x => x.Name == module.Name);

			if (foundModule is null) {
				return false;
			}

			(foundModule.Aliases ?? (foundModule.Aliases = new List<string>())).Add(alias);
			context.CommandStore.Update(foundModule);

			await context.CommandStore.SaveChangesAsync();

			module.Modify(OnBuilding(foundModule));

			return true;
		}

		async Task<bool> ICommandManagementService.AddAliasAsync(EspeonContext context, Module module, string command,
			string alias) {
			IReadOnlyList<Command> commands = this._commands.GetAllCommands();

			if (!Utilities.AvailableName(commands, alias)) {
				return false;
			}

			ModuleInfo foundModule = await context.CommandStore.Modules.Include(x => x.Commands)
				.FirstOrDefaultAsync(x => x.Name == module.Name);

			CommandInfo foundCommand = foundModule?.Commands.SingleOrDefault(x => x.Name == command);

			if (foundCommand is null) {
				return false;
			}

			(foundCommand.Aliases ?? (foundCommand.Aliases = new List<string>())).Add(alias);
			context.CommandStore.Update(foundModule);

			await context.CommandStore.SaveChangesAsync();

			module.Modify(OnBuilding(foundModule));

			return true;
		}

		async Task<bool> ICommandManagementService.
			RemoveAliasAsync(EspeonContext context, Module module, string alias) {
			ModuleInfo foundModule = await context.CommandStore.Modules.FindAsync(module.Name);

			if (foundModule?.Aliases is null || !foundModule.Aliases.Contains(alias)) {
				return false;
			}

			foundModule.Aliases.Remove(alias);
			context.CommandStore.Update(foundModule);

			await context.CommandStore.SaveChangesAsync();

			module.Modify(OnBuilding(foundModule));

			return true;
		}

		async Task<bool> ICommandManagementService.RemoveAliasAsync(EspeonContext context, Module module,
			string command, string alias) {
			ModuleInfo foundModule = await context.CommandStore.Modules.Include(x => x.Commands)
				.FirstOrDefaultAsync(x => x.Name == module.Name);

			CommandInfo foundCommand = foundModule?.Commands.SingleOrDefault(x => x.Name == command);

			if (foundCommand?.Aliases is null || !foundCommand.Aliases.Contains(alias)) {
				return false;
			}

			foundCommand.Aliases.Remove(alias);
			context.CommandStore.Update(foundModule);

			await context.CommandStore.SaveChangesAsync();

			module.Modify(OnBuilding(foundModule));

			return true;
		}

		private static Action<ModuleBuilder> OnBuilding(ModuleInfo info) {
			return builder => {
				if (!(info.Aliases is null) && info.Aliases.Count > 0) {
					builder.AddAliases(info.Aliases.ToArray());
				}

				foreach (CommandBuilder command in builder.Commands) {
					CommandInfo foundCommand = info.Commands.Find(x => x.Name == command.Name);

					if (!(foundCommand?.Aliases is null) && foundCommand.Aliases.Count > 0) {
						command.AddAliases(foundCommand.Aliases.ToArray());
					}
				}
			};
		}
	}
}