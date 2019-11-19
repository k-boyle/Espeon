using Disqord;
using Espeon.Commands;
using Espeon.Core;
using Espeon.Core.Database;
using Espeon.Core.Database.CommandStore;
using Espeon.Core.Database.GuildStore;
using Espeon.Core.Services;
using Kommon.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommandUtilities = Espeon.Commands.Utilities;
using CoreUtilities = Espeon.Core.Utilities;
using QmmandsUtilities = Qmmands.CommandUtilities;

namespace Espeon.Services {
	public class CommandHandlingService : BaseService<InitialiseArgs>, ICommandHandlingService {
		[Inject] private readonly DiscordClient _client;
		[Inject] private readonly CommandService _commands;
		[Inject] private readonly ICustomCommandsService _customCommands;
		[Inject] private readonly IEventsService _events;
		[Inject] private readonly ILogService _logger;
		[Inject] private readonly IMessageService _message;
		[Inject] private readonly IServiceProvider _services;

		private string[] _botMentions;

		public CommandHandlingService(IServiceProvider services) : base(services) {
			this._client.MessageReceived += async args => {
				if (args.Message is CachedUserMessage message) {
					await this._events.RegisterEvent(() => HandleMessageAsync(message));
				}
			};

			this._client.MessageUpdated += async args => {
				if (!args.OldMessage.HasValue) {
					return;
				}

				IMessage before = args.OldMessage.Value;
				IMessage after = args.NewMessage;
				
				if (before.Content == after.Content) {
					return;
				}

				if (after is CachedUserMessage message) {
					await this._events.RegisterEvent(() => HandleMessageAsync(message));
				}
			};

			this._commands.CommandExecutionFailed +=
				args => this._events.RegisterEvent(() => CommandExecutionFailedAsync(args));
			this._commands.CommandExecuted += args => this._events.RegisterEvent(() => CommandExecutedAsync(args));
		}

		async Task ICommandHandlingService.SetupCommandsAsync(CommandStore commandStore) {
			ModuleInfo[] dbModules = await commandStore.Modules.Include(x => x.Commands).ToArrayAsync();

			var modulesToCreate = new List<ModuleBuilder>();
			var commandsToCreate = new List<CommandBuilder>();

			IReadOnlyList<Module> modules = this._commands.AddModules(typeof(EspeonContext).Assembly,
				action: OnModuleBuilding(dbModules, modulesToCreate, commandsToCreate));

			foreach (ModuleBuilder module in modulesToCreate) {
				var newModule = new ModuleInfo { Name = module.Name };

				List<CommandBuilder> commandBuilders = module.Commands;
				if (module.Commands.Any(x => string.IsNullOrEmpty(x.Name))) {
					throw new NoNullAllowedException(nameof(module));
				}

				if (commandBuilders.Any(x => string.IsNullOrEmpty(x.Name))) {
					throw new NoNullAllowedException(nameof(commandBuilders));
				}

				newModule.Commands = commandBuilders.Select(x => new CommandInfo { Name = x.Name }).ToList();

				await commandStore.Modules.AddAsync(newModule);
			}

			foreach (CommandBuilder command in commandsToCreate) {
				ModuleInfo foundModule = await commandStore.Modules.FindAsync(command.Module.Name);

				foundModule.Commands.Add(new CommandInfo { Name = command.Name });

				commandStore.Update(foundModule);
			}

			await commandStore.SaveChangesAsync();

			this._services.GetService<IResponseService>()?.LoadResponses(modules);
		}

		private static Action<ModuleBuilder> OnModuleBuilding(ModuleInfo[] dbModules,
			List<ModuleBuilder> modulesToCreate, List<CommandBuilder> commandsToCreate) {
			return moduleBuilder => {
				if (string.IsNullOrEmpty(moduleBuilder.Name)) {
					throw new NoNullAllowedException(nameof(moduleBuilder));
				}

				ModuleInfo foundModule = Array.Find(dbModules, x => x.Name == moduleBuilder.Name);

				if (foundModule is null) {
					modulesToCreate.Add(moduleBuilder);
					return;
				}

				if (foundModule.Aliases?.Count > 0) {
					moduleBuilder.AddAliases(foundModule.Aliases);
				}

				foreach (CommandBuilder commandBuilder in moduleBuilder.Commands) {
					if (string.IsNullOrWhiteSpace(commandBuilder.Name)) {
						throw new NoNullAllowedException(nameof(commandBuilder));
					}

					CommandInfo foundCommand = foundModule.Commands.Find(x => x.Name == commandBuilder.Name);

					if (foundCommand is null) {
						commandsToCreate.Add(commandBuilder);
						continue;
					}

					if (foundCommand.Aliases?.Count > 0) {
						commandBuilder.AddAliases(foundCommand.Aliases);
					}
				}
			};
		}

		private async Task HandleMessageAsync(CachedUserMessage message) {
			await HandleMessageAsync(message.Author, message.Channel, message.Content, message);
		}

		private async Task HandleMessageAsync(CachedUser author, IMessageChannel channel, string content,
			CachedUserMessage message) {
			if (this._botMentions is null) {
				this._botMentions = new[] {
					$"<@!{this._client.CurrentUser.Id}> ",
					$"<@{this._client.CurrentUser.Id}> "
				};
			}

			if (author is null || author.IsBot && author.Id != this._client.CurrentUser.Id) {
				return;
			}

			if (!(channel is CachedTextChannel textChannel) || !textChannel.Guild.CurrentMember
				    .GetPermissionsFor(textChannel).Has(Permission.SendMessages)) {
				return;
			}

			IReadOnlyCollection<string> prefixes;

			using (var guildStore = this._services.GetService<GuildStore>()) {
				Guild guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild);
				prefixes = guild.Prefixes;

				if (guild.AutoQuotes) {
					_ = Task.Run(async () => {
						LocalEmbed embed = await CoreUtilities.QuoteFromStringAsync(this._client, content);

						if (embed is null) {
							return;
						}

						await channel.SendMessageAsync(string.Empty, embed: embed);
					});
				}

				if (guild.RestrictedChannels.Contains(textChannel.Id) || guild.RestrictedUsers.Contains(author.Id)) {
					return;
				}
			}


			if (QmmandsUtilities.HasAnyPrefix(content, prefixes, StringComparison.CurrentCulture, out string prefix,
				    out string output) ||
			    QmmandsUtilities.HasAnyPrefix(content, this._botMentions, out prefix, out output)) {
				if (string.IsNullOrWhiteSpace(output)) {
					return;
				}

				try {
					EspeonContext commandContext =
						await EspeonContext.CreateAsync(this._services, this._client, message, prefix);

					IResult result = await this._commands.ExecuteAsync(output, commandContext);

					bool CheckForCustom(Module module) {
						return result is ChecksFailedResult && ulong.TryParse(module.Name, out ulong id) &&
						       this._customCommands.IsCustomCommand(id);
					}

					if (result is CommandNotFoundResult || CheckForCustom(commandContext.Command.Module)) {
						commandContext = await EspeonContext.CreateAsync(this._services, this._client, message, prefix);
						result = await this._commands.ExecuteAsync($"help {output}", commandContext);
					}

					if (!result.IsSuccessful && !(result is ExecutionFailedResult)) {
						await CommandExecutionFailedAsync(new EspeonCommandErroredEventArgs {
							Context = commandContext,
							Result = result as FailedResult
						});
					}
				} catch (Exception ex) {
					this._logger.Log(Source.Commands, Severity.Error, string.Empty, ex);
				}
			}
		}

		async Task ICommandHandlingService.ExecuteCommandAsync(CachedUser author, ITextChannel channel, string content,
			CachedUserMessage message) {
			await HandleMessageAsync(author, channel, content, message);
		}

		private async Task CommandExecutionFailedAsync(EspeonCommandErroredEventArgs args) {
			EspeonContext context = args.Context;

			if (args.Result is ExecutionFailedResult failed) {
				this._logger.Log(Source.Commands, Severity.Error, string.Empty, failed.Exception);

#if !DEBUG
                var c = this._client.GetChannel(463299724326469634) as CachedTextChannel;

                var ex = failed.Exception.ToString();

                await c.SendMessageAsync(Markdown.EscapeMarkdown(ex.Length > 1000 ? ex.Substring(0, 1000) : ex));
#endif
			}

			await this._message.SendAsync(context.Message,
				x => x.Embed = CommandUtilities.BuildErrorEmbed(args.Result, context));
		}

		private Task CommandExecutedAsync(CommandExecutedEventArgs args) {
			var context = (EspeonContext) args.Context;

			this._logger.Log(Source.Commands, Severity.Verbose,
				$"Successfully executed {{{context.Command.Name}}} for " +
				$"{{{context.Member.DisplayName}}} in {{{context.Guild.Name}/{context.Channel.Name}}}");

			return Task.CompletedTask;
		}

		private struct EspeonCommandErroredEventArgs {
			public EspeonContext Context { get; set; }
			public FailedResult Result { get; set; }

			public static implicit operator EspeonCommandErroredEventArgs(CommandExecutionFailedEventArgs args) {
				return new EspeonCommandErroredEventArgs {
					Context = (EspeonContext) args.Context,
					Result = args.Result
				};
			}
		}
	}
}