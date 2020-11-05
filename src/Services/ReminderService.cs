using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Espeon {
    public class ReminderService : IOnReadyService {
        private const string ZeroWidthCharacter = "\u200b";

        private readonly IServiceProvider _services;
        private readonly ILogger<ReminderService> _logger;
        private readonly EspeonScheduler _scheduler;
        private readonly EspeonBot _espeon;

        public ReminderService(
                IServiceProvider services,
                EspeonScheduler scheduler,
                EspeonBot espeon,
                ILogger<ReminderService> logger) {
            this._services = services;
            this._logger = logger;
            this._scheduler = scheduler;
            this._espeon = espeon;
        }

        public async Task OnReadyAsync(EspeonDbContext context) {
            this._logger.LogInformation("Loading all reminders");
            // create a copy since we can't enumerate UserReminders and remove from it
            // it throws "connection is busy" which is not helpful at all if we attempt to
            // I could attempt to do some hacky stuff with throwing everything onto the 
            // scheduler... but that'll lead to an unholy amount of issues and very flakey
            var reminders = await context.UserReminders.ToListAsync();
            foreach (var reminder in reminders) {
                if (reminder.TriggerAt < DateTimeOffset.Now) {
                    this._logger.LogDebug("Sending missed reminder {reminder} for {user}", reminder.Id, reminder.UserId);
                    await OnReminderAync(context, reminder, true);
                } else {
                    SchedulerReminder(reminder);
                }
            }
        }

        public async Task CreateReminderAsync(UserReminder reminder) {
            this._logger.LogDebug("Creating reminder for {user}", reminder.UserId);
            using var scope = this._services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.PersistAsync(reminder);
            SchedulerReminder(reminder);
        }

        private void SchedulerReminder(UserReminder reminder) {
            this._logger.LogDebug("Scheduling reminder {reminder} for {user} at {at}", reminder.Id, reminder.UserId, reminder.TriggerAt);
            this._scheduler.DoAt(
                string.Concat("reminder-", reminder.UserId.ToString(), "-", reminder.Id),
                reminder.TriggerAt,
                (reminder, @this: this),
                async state => {
                    using var scope = state.@this!._services.CreateScope();
                    await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
                    await state.@this.OnReminderAync(context, state.reminder, false);
                });
        }

        private async Task OnReminderAync(EspeonDbContext context, UserReminder reminder, bool late) {
            if (this._espeon.GetChannel(reminder.ChannelId) is CachedTextChannel channel) {
                if (await channel.Guild.GetOrFetchMemberAsync(reminder.UserId) != null) {
                    this._logger.LogDebug("Sending reminder for {user}", reminder.UserId);
                    var originalMessage = await channel.GetMessageAsync(reminder.ReminderMessageId);
                    var embed = new LocalEmbedBuilder().WithColor(Constants.EspeonColour)
                        .WithDescription(reminder.Value)
                        .WithTitle(late ? "A (Late) Reminder" : "A Reminder")
                        .AddField(
                            ZeroWidthCharacter,
                            Markdown.Link("Original Message", originalMessage?.GetJumpUrl(channel.Guild)))
                        .WithFooter("Created")
                        .WithTimestamp(originalMessage?.CreatedAt)
                        .Build();
                    await channel.SendMessageAsync($"<@{reminder.UserId}>", embed: embed);
                } else {
                    this._logger.LogWarning("User {user} is not in guild for reminder {reminder}", reminder.UserId, reminder.Id);
                }
            } else {
                this._logger.LogWarning("Channel {channel} for reminder {reminder} was not found", reminder.ChannelId, reminder.Id);
            }

            await context.RemoveAsync(reminder);
        }
    }
}