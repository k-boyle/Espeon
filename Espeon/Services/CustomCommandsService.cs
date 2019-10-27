using Casino.DependencyInjection;
using Discord;
using Espeon.Commands;
using Espeon.Core;
using Espeon.Core.Databases;
using Espeon.Core.Databases.GuildStore;
using Espeon.Core.Services;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Utilities = Espeon.Commands.Utilities;

namespace Espeon.Services {
	public class CustomCommandsService : BaseService<InitialiseArgs>, ICustomCommandsService {
		[Inject] private readonly ILogService _log;
		[Inject] private readonly IMessageService _message;
		[Inject] private readonly CommandService _commands;

		private readonly ConcurrentDictionary<ulong, Module> _moduleCache;

		public CustomCommandsService(IServiceProvider services) : base(services) {
			this._moduleCache = new ConcurrentDictionary<ulong, Module>();
		}

		public override async Task InitialiseAsync(IServiceProvider services, InitialiseArgs args) {
			IReadOnlyCollection<Guild> guilds = await args.GuildStore.GetAllGuildsAsync(x => x.Commands);

			IEnumerable<Task> createCommands = guilds.Select(CreateCommandsAsync);

			await Task.WhenAll(createCommands);
			this._log.Log(Source.Commands, Severity.Verbose, "All custom commands loaded");
		}

		private Task CreateCommandsAsync(Guild guild) {
			List<CustomCommand> commands = guild.Commands;

			if (commands is null || commands.Count == 0) {
				return Task.CompletedTask;
			}

			this._moduleCache[guild.Id] = this._commands.AddModule(moduleBuilder => {
				moduleBuilder.Name = guild.Id.ToString();
				moduleBuilder.AddCheck(new RequireGuildAttribute(guild.Id));

				foreach (CustomCommand command in commands) {
					if (command.Name is null) {
						continue;
					}

					moduleBuilder.AddCommand(CommandCallbackAsync, commandBuilder => {
						commandBuilder.Name = command.Name;
						commandBuilder.AddAlias(command.Name);
					});
				}
			});

			return Task.CompletedTask;
		}

		private async ValueTask CommandCallbackAsync(CommandContext originalContext) {
			var context = (EspeonContext) originalContext;

			Guild guild = await context.GuildStore.GetOrCreateGuildAsync(context.Guild, x => x.Commands);
			List<CustomCommand> commands = guild?.Commands;

			CustomCommand found = commands?.FirstOrDefault(x =>
				string.Equals(x.Name, context.Command.Name, StringComparison.InvariantCultureIgnoreCase));

			await this._message.SendAsync(context.Message, x => x.Content = found?.Value);
		}

		async Task<bool> ICustomCommandsService.TryCreateCommandAsync(GuildStore guildStore, IGuild guild, string name, string value) {
			if (this._moduleCache.TryGetValue(guild.Id, out Module found)) {
				IReadOnlyList<Command> commands = found.Commands;

				if (!Utilities.AvailableName(commands, name)) {
					return false;
				}
			}

			IReadOnlyList<Command> loadedCommands = this._commands.GetAllCommands();
			IEnumerable<Command> filtered = loadedCommands.Where(x => !ulong.TryParse(x.Module.Name, out _));

			if (!Utilities.AvailableName(filtered, name)) {
				return false;
			}

			var newCmd = new CustomCommand {
				Name = name,
				Value = value
			};

			Guild dbGuild = await guildStore.GetOrCreateGuildAsync(guild, x => x.Commands);
			dbGuild.Commands.Add(newCmd);
			guildStore.Update(dbGuild);

			await guildStore.SaveChangesAsync();
			await UpdateCommandsAsync(dbGuild);

			return true;
		}

		async Task ICustomCommandsService.DeleteCommandAsync(GuildStore guildStore, IGuild guild, CustomCommand command) {
			Guild dbGuild = await guildStore.GetOrCreateGuildAsync(guild, x => x.Commands);
			dbGuild.Commands.Remove(command);
			guildStore.Update(dbGuild);

			await guildStore.SaveChangesAsync();

			await UpdateCommandsAsync(dbGuild);
		}

		Task ICustomCommandsService.ModifyCommandAsync(GuildStore guildStore, CustomCommand command, string newValue) {
			command.Value = newValue;
			guildStore.Update(command);

			return guildStore.SaveChangesAsync();
		}

		async Task<ImmutableArray<CustomCommand>> ICustomCommandsService.GetCommandsAsync(GuildStore guildStore, IGuild guild) {
			Guild dbGuild = await guildStore.GetOrCreateGuildAsync(guild, x => x.Commands);
			return dbGuild.Commands.ToImmutableArray();
		}

		bool ICustomCommandsService.IsCustomCommand(ulong id) {
			return this._moduleCache.TryGetValue(id, out _);
		}

		private async Task UpdateCommandsAsync(Guild guild) {
			if (this._moduleCache.ContainsKey(guild.Id)) {
				this._commands.RemoveModule(this._moduleCache[guild.Id]);
			}

			await CreateCommandsAsync(guild);
		}
	}
}