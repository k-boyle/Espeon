using Casino.Common;
using Casino.DependencyInjection;
using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Databases;
using Espeon.Core.Databases.CommandStore;
using Espeon.Core.Databases.GuildStore;
using Espeon.Core.Databases.UserStore;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
	public class BotStartup {
		private readonly IServiceProvider _services;

		[Inject] private readonly ICommandHandlingService _commands;
		[Inject] private readonly DiscordSocketClient _client;
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

			await this._client.LoginAsync(TokenType.Bot, this._config.DiscordToken);
			await this._client.StartAsync();

			await this._tcs.Task;
		}

		private void EventHooks(UserStore userStore) {
			var logger = this._services.GetService<ILogService>();

			async Task ReadyAsync() {
				await this._services.GetService<IReminderService>().LoadRemindersAsync(userStore);
				_ = Task.Run(() => this._services.GetService<IStatusService>().RunStatusesAsync());

				this._client.Ready -= ReadyAsync;
				this._tcs.SetResult(true);

#if DEBUG
				Console.Beep(5000, 100);
#endif
			}

			this._client.Ready += ReadyAsync;

			this._client.UserJoined += user => this._events.RegisterEvent(async () => {
				using var guildStore = this._services.GetService<GuildStore>();

				Guild dbGuild = await guildStore.GetOrCreateGuildAsync(user.Guild);
				SocketGuild guild = user.Guild;

				if (guild.GetTextChannel(dbGuild.WelcomeChannelId) is { } channel &&
				    !string.IsNullOrWhiteSpace(dbGuild.WelcomeMessage)) {
					string str = dbGuild.WelcomeMessage.Replace("{{guild}}", user.Guild.Name)
						.Replace("{{user}}", user.GetDisplayName());

					await channel.SendMessageAsync(user.Mention, embed: new EmbedBuilder {
						Title = "A User Appears!",
						Color = Utilities.EspeonColor,
						Description = str,
						ThumbnailUrl = user.GetAvatarOrDefaultUrl()
					}.Build());
				}

				if (guild.GetRole(dbGuild.DefaultRoleId) is { } role) {
					await user.AddRoleAsync(role, new RequestOptions { AuditLogReason = "Auto role on join" });
				}
			});

			this._client.JoinedGuild += guild => this._events.RegisterEvent(async () => {
				var channelName = new[] {
					"welcome",
					"introduction",
					"general"
				};

				SocketTextChannel channel =
					guild.TextChannels.FirstOrDefault(x =>
						channelName.Any(y => x.Name.Contains(y, StringComparison.InvariantCultureIgnoreCase))) ??
					guild.TextChannels.FirstOrDefault(x =>
						guild.CurrentUser.GetPermissions(x).ViewChannel &&
						guild.CurrentUser.GetPermissions(x).SendMessages);

				if (channel is null) {
					return;
				}

				await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder {
					Title = "",
					Color = Utilities.EspeonColor,
					ThumbnailUrl = guild.CurrentUser.GetAvatarOrDefaultUrl(),
					Description =
						$"Hello! I am Espeon.Core{this._services.GetService<IEmoteService>().Collection["Espeon"]} " +
						"and I have just been added to your guild!\n" + "Type es/help to see all my available commands!"
				}.Build());
			});

			this._client.Log += log => this._events.RegisterEvent(() => {
				logger.Log(Source.Discord, (Severity) (int) log.Severity, log.Message, log.Exception);
				return Task.CompletedTask;
			});

			this._services.GetService<TaskQueue>().OnError += ex => this._events.RegisterEvent(() => {
				logger.Log(Source.Scheduler, Severity.Error, string.Empty, ex);
				return Task.CompletedTask;
			});
		}
	}
}