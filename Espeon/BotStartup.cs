using Casino.Common;
using Casino.DependencyInjection;
using Disqord;
using Disqord.Events;
using Espeon.Core;
using Espeon.Core.Database;
using Espeon.Core.Database.CommandStore;
using Espeon.Core.Database.GuildStore;
using Espeon.Core.Database.UserStore;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
	public class BotStartup {
		private readonly IServiceProvider _services;

		[Inject] private readonly ICommandHandlingService _commands;
		[Inject] private readonly DiscordClient _client;
		[Inject] private readonly IEventsService _events;

		private readonly Config _config;

		private readonly TaskCompletionSource<bool> _tcs;

		public BotStartup(IServiceProvider services, Config config) {
			this._services = services;
			this._config = config;

			this._tcs = new TaskCompletionSource<bool>();
		}

		public async Task StartAsync(UserStore userStore, CommandStore commandStore) {
			EventHooks(userStore);

			await this._commands.SetupCommandsAsync(commandStore);

			await this._client.ConnectAsync();

			await this._tcs.Task;
		}

		private void EventHooks(UserStore userStore) {
			var logger = this._services.GetService<ILogService>();

			async Task ReadyAsync(ReadyEventArgs eventArgs) {
				await this._services.GetService<IReminderService>().LoadRemindersAsync(userStore);
				_ = Task.Run(() => this._services.GetService<IStatusService>().RunStatusesAsync());

				this._client.Ready -= ReadyAsync;
				this._tcs.SetResult(true);

#if DEBUG
				Console.Beep(5000, 100);
#endif
			}

			this._client.Ready += ReadyAsync;

			this._client.MemberJoined += eventArgs => this._events.RegisterEvent(async () => {
				await using var guildStore = this._services.GetService<GuildStore>();
				CachedMember member = eventArgs.Member;
				CachedGuild guild = member.Guild;

				Guild dbGuild = await guildStore.GetOrCreateGuildAsync(guild);

				if (guild.GetTextChannel(dbGuild.WelcomeChannelId) is { } channel &&
				    !string.IsNullOrWhiteSpace(dbGuild.WelcomeMessage)) {
					string str = dbGuild.WelcomeMessage.Replace("{{guild}}", guild.Name)
						.Replace("{{user}}", member.DisplayName);

					await channel.SendMessageAsync(member.Mention,
						embed: new LocalEmbedBuilder {
							Title = "A User Appears!",
							Color = Utilities.EspeonColor,
							Description = str,
							ThumbnailUrl = member.GetAvatarUrl()
						}.Build());
				}

				if (guild.GetRole(dbGuild.DefaultRoleId) is { } role) {
					await member.GrantRoleAsync(role.Id, RestRequestOptions.FromReason("Auto role on join"));
				}
			});

			this._client.JoinedGuild += eventArgs => this._events.RegisterEvent(async () => {
				CachedGuild guild = eventArgs.Guild;
				var channelNames = new[] {
					"welcome",
					"introduction",
					"general"
				};

				CachedTextChannel channel =
					guild.TextChannels.FirstOrDefault(x =>
							channelNames.Any(y => 
								x.Value.Name.Contains(y, StringComparison.InvariantCultureIgnoreCase))).Value 
				 ?? guild.TextChannels.FirstOrDefault(x =>
						guild.CurrentMember.GetPermissionsFor(x.Value).ViewChannel &&
						guild.CurrentMember.GetPermissionsFor(x.Value).SendMessages).Value;

				if (channel is null) {
					return;
				}

				await channel.SendMessageAsync(string.Empty, embed: new LocalEmbedBuilder() {
					Title = "",
					Color = Utilities.EspeonColor,
					ThumbnailUrl = guild.CurrentMember.DisplayName,
					Description =
						$"Hello! I am Espeon.Core{this._services.GetService<IEmoteService>()["Espeon"]} " +
						"and I have just been added to your guild!\n" + "Type es/help to see all my available commands!"
				}.Build());
			});

			this._client.Logger.MessageLogged += (obj, eventArgs) => this._events.RegisterEvent(() => {
				logger.Log(Source.Discord, (Severity) (int) eventArgs.Severity, eventArgs.Message, eventArgs.Exception);
				return Task.CompletedTask;
			});

			this._services.GetService<TaskQueue>().OnError += ex => this._events.RegisterEvent(() => {
				logger.Log(Source.Scheduler, Severity.Error, string.Empty, ex);
				return Task.CompletedTask;
			});
		}
	}
}