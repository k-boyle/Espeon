using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Humanizer;
using Humanizer.Localisation;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var reminders = (await DbContext.GetRemindersAsync(IsValidReminder))
                .OrderBy(reminder => reminder.TriggerAt)
                .ToList();
            
            if (reminders.Count == 0) {
                await ReplyAsync(NO_REMINDERS_FOUND);
                return;
            }
            
            if (reminders.Count <= BatchSize) {
                var embed = CreateReminderEmbed(reminders);
                await ReplyAsync(embed: embed.Build());
                return;
            }

            var numberOfPages = MathEx.IntCeilingDivision(reminders.Count, BatchSize);
            var reminderPages = reminders.Batch(BatchSize)
                .Select((reminders, index) => {
                    var reminderEmbedBuilder = CreateReminderEmbed(reminders);
                    reminderEmbedBuilder.Footer = new LocalEmbedFooterBuilder {
                        Text = $"Page [{index + 1}/{numberOfPages}]" 
                    };
                    return new Page(reminderEmbedBuilder.Build());
                });
            var pageProvder = new DefaultPageProvider(reminderPages);
            var menu = new PagedMenu(Context.User.Id, pageProvder);
            await Context.Channel.StartMenuAsync(menu);
        }

        private bool IsValidReminder(UserReminder reminder) {
            return reminder.UserId == Context.Member.Id && Context.Guild.Channels.ContainsKey(reminder.ChannelId);
        }
        
        private LocalEmbedBuilder CreateReminderEmbed(IEnumerable<UserReminder> reminders) {
            string FormatReminderString(UserReminder reminder, int index) {
                var executesIn = reminder.TriggerAt - DateTimeOffset.Now;
                var channelString = Context.Guild.Channels.TryGetValue(reminder.ChannelId, out var channel)
                    ? ((CachedTextChannel) channel).Mention
                    : "Channel Deleted";
                var valueString = reminder.Value.Length < 100
                    ? reminder.Value
                    : $"{reminder.Value.Substring(0, 97)}...";

                return $"{Markdown.Bold("Id")}: {index}\n"
                     + $"{Markdown.Bold("Executes In")}: {executesIn.Humanize(2, minUnit: TimeUnit.Second)}\n"
                     + $"{Markdown.Bold("Channel")}: {channelString}\n"
                     + $"{Markdown.Bold("Reminder")}: {valueString}\n";
            }
                
            var reminderEmbedBuilder = new LocalEmbedBuilder {
                Color = Constants.EspeonColour,
                Title = $"{Context.Member.DisplayName}'s Reminders",
                Description = string.Join('\n', reminders.Select(FormatReminderString))
            };
                
            return reminderEmbedBuilder;
        }

        public async ValueTask DisposeAsync() {
            await DbContext.DisposeAsync();
        }
    }
}