using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Espeon {
    public class ReminderService : IOnReadyService {
        private const string ZeroWidthCharacter = "\u200b";

        private readonly IServiceProvider _services;
        private readonly ILogger<ReminderService> _logger;
        private readonly EspeonScheduler _scheduler;
        private readonly EspeonBot _espeon;
        private readonly ConcurrentDictionary<string, ScheduledTask<(UserReminder, ReminderService)>> _scheduledReminderById;
        private readonly ConcurrentDictionary<ulong, HashSet<UserReminder>> _reminderByUserId;

        public ReminderService(
            IServiceProvider services,
            EspeonScheduler scheduler,
            EspeonBot espeon,
            ILogger<ReminderService> logger) {
            this._services = services;
            this._logger = logger;
            this._scheduler = scheduler;
            this._espeon = espeon;
            this._scheduledReminderById = new();
            this._reminderByUserId = new();
        }

        public async Task OnReadyAsync(EspeonDbContext context) {
            this._logger.LogInformation("Loading all reminders");
            var reminders = await context.UserReminders.ToListAsync();
            foreach (var reminder in reminders) {
                if (reminder.TriggerAt < DateTimeOffset.Now) {
                    this._logger.LogDebug("Sending missed reminder {reminder} for {user}", reminder.Id, reminder.UserId);
                    await OnReminderAsync(context, reminder, true);
                } else {
                    ScheduleReminder(reminder);
                }
            }
        }

        public async Task CreateReminderAsync(UserReminder reminder) {
            this._logger.LogDebug("Creating reminder for {user}", reminder.UserId);
            using var scope = this._services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.PersistAsync(reminder);
            ScheduleReminder(reminder);
        }

        public async Task CancelReminderAsync(EspeonDbContext context, UserReminder reminder) {
            this._logger.LogDebug("Cancelling reminder {reminder}", reminder.Id);
            if (this._scheduledReminderById.TryRemove(reminder.Id, out var task)) {
                task.Cancel();
                await context.RemoveAsync(reminder);
                this._reminderByUserId.AddOrUpdate(
                    reminder.UserId,
                    (_, __) => new HashSet<UserReminder>(),
                    (_, set, reminder) => {
                        set.Remove(reminder);
                        return set;
                    },
                    reminder
                );
            }
        }

        public IEnumerable<UserReminder> GetRemindersForUser(ulong userId) {
            return this._reminderByUserId.TryGetValue(userId, out var reminders) ? reminders : null;
        }

        private void ScheduleReminder(UserReminder reminder) {
            this._logger.LogDebug("Scheduling reminder {reminder} for {user} at {at}", reminder.Id, reminder.UserId, reminder.TriggerAt);
            this._reminderByUserId.AddOrUpdate(
                reminder.UserId,
                (_, reminder) => new HashSet<UserReminder> {
                    reminder
                },
                (_, set, reminder) => {
                    set.Add(reminder);
                    return set;
                },
                reminder
            );
            this._scheduledReminderById[reminder.Id] = this._scheduler.DoAt(
                string.Concat("reminder-", reminder.UserId.ToString(), "-", reminder.Id),
                reminder.TriggerAt,
                (reminder, @this: this),
                async state => {
                    var (userReminder, reminderService) = state;
                    using var scope = reminderService._services.CreateScope();
                    await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
                    await reminderService.OnReminderAsync(context, userReminder, false);
                }
            );
        }

        private async Task OnReminderAsync(EspeonDbContext context, UserReminder reminder, bool late) {
            if (GetReminderChannel(reminder) is not CachedTextChannel channel) {
                this._logger.LogWarning("Channel {channel} for reminder {reminder} was not found", reminder.ChannelId, reminder.Id);
            } else {
                if (await channel.Guild.GetOrFetchMemberAsync(reminder.UserId) == null) {
                    this._logger.LogWarning("User {user} is not in guild for reminder {reminder}", reminder.UserId, reminder.Id);
                } else {
                    await SendReminderAsync(reminder, late, channel);
                }
            }

            await context.RemoveAsync(reminder);
            this._scheduledReminderById.TryRemove(reminder.Id, out _);
        }

        private async Task SendReminderAsync(UserReminder reminder, bool late, CachedTextChannel channel) {
            this._logger.LogDebug("Sending reminder for {user}", reminder.UserId);
            var originalMessage = await channel.GetMessageAsync(reminder.ReminderMessageId);
            var embed = new LocalEmbedBuilder()
                .WithColor(Constants.EspeonColour)
                .WithDescription(reminder.Value)
                .WithTitle(late ? "A (Late) Reminder" : "A Reminder")
                .AddField(
                    ZeroWidthCharacter,
                    Markdown.Link("Original Message", originalMessage?.GetJumpUrl(channel.Guild))
                )
                .WithFooter("Created")
                .WithTimestamp(originalMessage?.CreatedAt)
                .Build();
            await channel.SendMessageAsync($"<@{reminder.UserId}>", embed: embed);
        }

        private CachedChannel GetReminderChannel(UserReminder reminder) {
            var guild = this._espeon.GetGuild(reminder.GuildId);
            var channel = guild?.GetTextChannel(reminder.ChannelId);
            return channel ?? this._espeon.GetChannel(reminder.ChannelId);
        }
    }
}