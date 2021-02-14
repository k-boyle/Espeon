using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    [Name("Reminders")]
    [Description("Forgetful? Reminders")]
    [Group("reminder", "reminders", "remindme", "remind", "rm", "r")]
    public class ReminderModule : EspeonCommandModule, IAsyncDisposable {
        private const int BatchSize = 4;
        
        public ReminderService ReminderService { get; set; }
        public EspeonDbContext DbContext { get; set; }
        
        [Name("Create Reminder")]
        [Description("Creates a reminders")]
        [Command("", "create", "c")]
        public async Task CreateReminderAsync([Remainder] UserReminder reminder) {
            await ReminderService.CreateReminderAsync(reminder);
            await ReplyAsync(REMINDER_CREATED);
        }
        
        [Name("List Reminders")]
        [Description("Lists your reminders")]
        [Command("list", "ls", "l")]
        public async Task ListRemindersAsync() {
            await DbContext.UserReminders.LoadAsync();
            var reminders = DbContext.UserReminders
                .Where(IsValidReminder)
                .OrderBy(reminder => reminder.TriggerAt)
                .ToList();
            
            switch (reminders.Count) {
                case 0:
                    await ReplyAsync(NO_REMINDERS_FOUND);
                    return;

                case <= BatchSize: 
                    var embed = await CreateReminderEmbedAsync(reminders);
                    await ReplyAsync(embed: embed.Build());
                    return;
            }

            var numberOfPages = MathEx.CeilingDivision(reminders.Count, BatchSize);
            var reminderBatches = reminders.Batch(BatchSize);
            var pageIndex = 0;
            var reminderPages = new List<Page>();
            foreach (var reminderBatch in reminderBatches) {
                var reminderEmbedBuilder = await CreateReminderEmbedAsync(reminderBatch);
                reminderEmbedBuilder.Footer = new LocalEmbedFooterBuilder {
                    Text = $"Page [{++pageIndex}/{numberOfPages}]"
                };
                reminderPages.Add(reminderEmbedBuilder.Build());
            }
            var pageProvider = new DefaultPageProvider(reminderPages);
            var menu = new PagedMenu(Context.User.Id, pageProvider);
            await Context.Channel.StartMenuAsync(menu);
        }

        private bool IsValidReminder(UserReminder reminder) {
            return reminder.UserId == Context.Member.Id && Context.Guild.Channels.ContainsKey(reminder.ChannelId);
        }
        
        private async Task<LocalEmbedBuilder> CreateReminderEmbedAsync(IEnumerable<UserReminder> reminders) {
            var reminderStringBuilder = new StringBuilder();
            var index = 0;
            foreach (var reminder in reminders) {
                var reminderString = await FormatReminderStringAsync(reminder, index++);
                reminderStringBuilder.AppendLine(reminderString);
            }
            
            var reminderEmbedBuilder = new LocalEmbedBuilder {
                Color = Constants.EspeonColour,
                Title = $"{Context.Member.DisplayName}'s Reminders",
                Description = reminderStringBuilder.ToString()
            };

            return reminderEmbedBuilder;
        }

        private async Task<string> FormatReminderStringAsync(UserReminder reminder, int index) {
            var reminderStringBuilder = new StringBuilder();
            var executesIn = reminder.TriggerAt - DateTimeOffset.Now;
            reminderStringBuilder.AppendLine($"{Markdown.Bold("Id")}: {index}");
            reminderStringBuilder.AppendLine(
                $"{Markdown.Bold("Executes In")}: {executesIn.Humanize(1, minUnit: Humanizer.Localisation.TimeUnit.Second)}");
            var valueString = reminder.Value.Length < 100 
                ? reminder.Value 
                : $"{reminder.Value.Substring(0, 97)}...";
            reminderStringBuilder.AppendLine($"{Markdown.Bold("Reminder")}: {valueString}");

            if (!Context.Guild.Channels.TryGetValue(reminder.ChannelId, out var ch) || !(ch is CachedTextChannel channel)) {
                return reminderStringBuilder.ToString();
            }

            reminderStringBuilder.AppendLine($"{Markdown.Bold("Channel")}: {channel.Mention}");

            var message = await channel.GetMessageAsync(reminder.ReminderMessageId);

            if (message == null) {
                return reminderStringBuilder.ToString();
            }

            var jumpUrl = message.GetJumpUrl(channel.Guild);
            var linkMarkdown = Markdown.Link(reminder.ReminderMessageId.ToString(), jumpUrl);
            reminderStringBuilder.AppendLine($"{Markdown.Bold("Original Message")}: {linkMarkdown}");

            return reminderStringBuilder.ToString();
        }

        public async ValueTask DisposeAsync() {
            await DbContext.DisposeAsync();
        }
    }
}