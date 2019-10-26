using Casino.Common;
using Casino.DependencyInjection;
using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Commands;
using Espeon.Core.Databases;
using Espeon.Core.Databases.UserStore;
using Espeon.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Reminder = Espeon.Core.Databases.Reminder;

namespace Espeon.Services {
	public class ReminderService : BaseService<InitialiseArgs>, IReminderService {
		[Inject] private readonly ILogService _logger;
		[Inject] private readonly TaskQueue _scheduler;
		[Inject] private readonly IServiceProvider _services;
		[Inject] private readonly DiscordSocketClient _client;

		private readonly ConcurrentDictionary<string, ScheduledTask<Reminder>> _reminders;

		public ReminderService(IServiceProvider services) : base(services) {
			this._reminders = new ConcurrentDictionary<string, ScheduledTask<Reminder>>(1, 10);
		}

		//requires client to be populated to send reminders
		async Task IReminderService.LoadRemindersAsync(UserStore ctx) {
			this._logger.Log(Source.Reminders, Severity.Info, "Sending all missed reminders");

			Reminder[] reminders = await ctx.Reminders.ToArrayAsync();
			foreach (Reminder reminder in reminders) {
				if (DateTimeOffset.UtcNow > reminder.WhenToRemove) {
					await RemoveAsync(reminder);
					continue;
				}

				ScheduledTask<Reminder> task =
					this._scheduler.ScheduleTask(reminder, reminder.WhenToRemove, RemoveAsync);
				this._reminders.TryAdd(reminder.Id, task);
			}
		}

		async Task<Reminder> IReminderService.
			CreateReminderAsync(EspeonContext context, string content, TimeSpan when) {
			Reminder[] reminders = await context.UserStore.Reminders.ToArrayAsync();
			Reminder[] usersReminders = reminders.Where(x => x.UserId == context.User.Id).ToArray();
			int next = usersReminders.Length == 0 ? 0 : usersReminders.Max(x => x.ReminderId) + 1;

			Reminder found = Array.Find(usersReminders, x => x.InvokeId == context.Message.Id);

			ScheduledTask<Reminder> task;

			if (found is null) {
				var reminder = new Reminder {
					ChannelId = context.Channel.Id,
					GuildId = context.Guild.Id,
					JumpUrl = context.Message.GetJumpUrl(),
					TheReminder = content,
					UserId = context.User.Id,
					WhenToRemove = DateTimeOffset.UtcNow.Add(when),
					ReminderId = next,
					InvokeId = context.Message.Id,
					Id = Guid.NewGuid().ToString(),
					CreatedAt = DateTimeOffset.UtcNow
				};

				task = this._scheduler.ScheduleTask(reminder, reminder.WhenToRemove, RemoveAsync);
				this._reminders.TryAdd(reminder.Id, task);

				await context.UserStore.Reminders.AddAsync(reminder);
				await context.UserStore.SaveChangesAsync();

				return reminder;
			}

			found.TheReminder = content;
			found.WhenToRemove = DateTimeOffset.UtcNow.Add(when);

			context.UserStore.Reminders.Update(found);
			await context.UserStore.SaveChangesAsync();

			if (this._reminders.TryGetValue(found.Id, out task)) {
				task.Change(when, _ => RemoveAsync(found));
			}

			return found;
		}

		async Task IReminderService.CancelReminderAsync(EspeonContext context, Reminder reminder) {
			context.UserStore.Remove(reminder);

			if (this._reminders.TryGetValue(reminder.Id, out ScheduledTask<Reminder> task)) {
				task.Cancel();
			}

			await context.UserStore.SaveChangesAsync();
		}

		async Task<ImmutableArray<Reminder>> IReminderService.GetRemindersAsync(EspeonContext context) {
			User user = await context.UserStore.GetOrCreateUserAsync(context.User, x => x.Reminders);
			return user.Reminders.ToImmutableArray();
		}

		private async Task RemoveAsync(Reminder reminder) {
			if (!(this._client.GetGuild(reminder.GuildId) is { } guild)) {
				return;
			}

			if (!(this._client.GetChannel(reminder.ChannelId) is SocketTextChannel channel)) {
				return;
			}

			if (!(guild.GetUser(reminder.UserId) is IGuildUser user)) {
				return;
			}

			Embed embed = ResponseBuilder.Reminder(user, ReminderString(reminder.TheReminder, reminder.JumpUrl),
				DateTimeOffset.UtcNow - reminder.CreatedAt);

			await channel.SendMessageAsync(user.Mention, embed: embed);

			using var ctx = this._services.GetService<UserStore>();

			ctx.Reminders.Remove(reminder);

			await ctx.SaveChangesAsync();

			this._logger.Log(Source.Reminders, Severity.Verbose,
				$"Sent reminder for {{{user.GetDisplayName()}}} in {{{guild.Name}}}/{{{channel.Name}}}");
		}

		private static string ReminderString(string reminder, string jumpUrl) {
			return $"{reminder}\n\n[Original Message]({jumpUrl})";
		}
	}
}