using Discord;
using Espeon.Core;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DR = Espeon.Core.Databases.Reminder;

namespace Espeon.Commands {
	[Name("Reminders")]
	[Group("Reminder")]
	[Description("Create reminders for yourself")]
	public class Reminder : EspeonModuleBase {
		public IReminderService ReminderService { get; set; }

		[Command]
		[Name("New Reminder")]
		[Description("Creates a new reminder")]
		public Task CreateReminderAsync([Remainder] (string Content, TimeSpan Time) reminder) {
			return Task.WhenAll(ReminderService.CreateReminderAsync(Context, reminder.Content, reminder.Time),
				SendOkAsync(0, reminder.Time.Humanize()));
		}

		[Command("List")]
		[Name("List Reminders")]
		[Description("Lists all of your currently available reminders")]
		public async Task ListRemindersAsync() {
			ImmutableArray<DR> reminders = await ReminderService.GetRemindersAsync(Context);

			if (reminders.Length == 0) {
				await SendOkAsync(0);
				return;
			}

			IOrderedEnumerable<DR> ordered =
				reminders.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.WhenToRemove);

			static string ReminderStr(DR reminder) {
				TimeSpan @in = reminder.WhenToRemove - DateTimeOffset.UtcNow;
				string content = reminder.TheReminder;

				string str = content.Length > 50 ? $"{content.Substring(0, 47)}..." : content;

				return $"<reminder id=\"{reminder.ReminderId}\"> \n\t• In {@in.Humanize()}; \n\t• {str}";
			}

			IEnumerable<string> strs = ordered.Select(ReminderStr);
			string joint = string.Join("\n\n", strs);

			List<string> split = Utilities.SplitByLength(joint, 900);

			if (split.Count == 1) {
				await SendMessageAsync($"```md\n{split[0]}\n```");
				return;
			}

			IEnumerable<string> responses = split.Select(x => $"```md\n{x}\n```");

			var index = 0;
			PaginatorOptions pOptions =
				PaginatorOptions.Default(responses.ToDictionary(_ => index++, x => (x, (Embed) null)));

			await TryAddCallbackAsync(new DefaultPaginator(Context, Interactive, Message, pOptions,
				new ReactionFromSourceUser(Context.User.Id)));
		}

		[Command("Cancel")]
		[Name("Cancel Reminder")]
		[Description("Cancel the specified reminder")]
		public async Task CancelReminderAsync(int reminderId) {
			DR found = await Context.UserStore.Reminders.FirstOrDefaultAsync(x =>
				x.UserId == Context.User.Id && x.ReminderId == reminderId);

			if (found is null) {
				await SendNotOkAsync(0, reminderId);
				return;
			}

			await ReminderService.CancelReminderAsync(Context, found);

			await SendOkAsync(1);
		}
	}
}