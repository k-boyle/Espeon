using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Espeon {
    public class ReminderService : IOnReadyService {
        private const string ZeroWidthCharacter = "\u200b";

        public IServiceProvider Provider { get; }
        public ILogger Logger { get; }

        private readonly EspeonScheduler _scheduler;
        private readonly EspeonBot _espeon;

        public ReminderService(IServiceProvider services, EspeonScheduler scheduler, EspeonBot espeon, ILogger logger) {
            Provider = services;
            Logger = logger.ForContext("SourceContext", nameof(ReminderService));

            this._scheduler = scheduler;
            this._espeon = espeon;
        }

        public async Task OnReadyAsync(EspeonDbContext context) {
            Logger.Information("Loading all reminders");
            var reminders = await context.GetRemindersAsync();
            foreach (var reminder in reminders) {
                if (reminder.TriggerAt < DateTimeOffset.Now) {
                    Logger.Debug("Sending missed reminder {reminder} for {user}", reminder.Id, reminder.UserId);
                    await OnReminderAync(context, reminder, true);
                } else {
                    Logger.Debug("Scheduling reminder {reminder} for {user} at {at}", reminder.Id, reminder.UserId, reminder.TriggerAt);
                    this._scheduler.DoAt(
                        string.Concat("reminder-", reminder.UserId.ToString(), "-", reminder.Id),
                        reminder.TriggerAt,
                        (reminder, Provider),
                        async state => {
                            using var scope = state.Provider!.CreateScope();
                            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
                            await OnReminderAync(context, state.reminder, false);
                        });
                }
            }
        }
        
        public async Task CreateReminderAsync(UserReminder reminder) {
            Logger.Debug("Creating reminder for {user}", reminder.UserId);
            using var scope = Provider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.PersistAsync(reminder);
            this._scheduler.DoAt(
                string.Concat("reminder-", reminder.UserId.ToString(), "-", reminder.Id),
                reminder.TriggerAt,
                (reminder, Provider),
                async state => {
                    using var scope = state.Provider!.CreateScope();
                    await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
                    await OnReminderAync(context, state.reminder, false);
                });
        }
        
        private async Task OnReminderAync(EspeonDbContext context, UserReminder reminder, bool late) {
            if (this._espeon.GetChannel(reminder.ChannelId) is CachedTextChannel channel
                    && channel.Guild.GetMember(reminder.UserId) is { }) {
                Logger.Debug("Sending reminder for {user}", reminder.UserId);
                var originalMessage = await channel.GetMessageAsync(reminder.ReminderMessageId);
                var embed = new LocalEmbedBuilder()
                    .WithColor(Constants.EspeonColour)
                    .WithDescription(reminder.Value)
                    .WithTitle(late ? "A (Late) Reminder" : "A Reminder")
                    .AddField(ZeroWidthCharacter, Markdown.Link("Original Message", originalMessage?.GetJumpUrl(channel.Guild)))
                    .WithFooter("Created")
                    .WithTimestamp(originalMessage?.CreatedAt)
                    .Build();
                await channel.SendMessageAsync($"<@{reminder.UserId}>", embed: embed);
            }

            await context.RemoveAsync(reminder);
        }
    }
}