using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Espeon {
    public class ReminderService : IOnReadyService {
        private const string ZeroWidthCharacter = "\u200b";
        
        private readonly IServiceProvider _services;
        private readonly EspeonScheduler _scheduler;
        private readonly EspeonBot _espeon;
        private readonly ILogger _logger;

        public ReminderService(IServiceProvider services, EspeonScheduler scheduler, EspeonBot espeon, ILogger logger) {
            this._services = services;
            this._scheduler = scheduler;
            this._espeon = espeon;
            this._logger = logger.ForContext("SourceContext", nameof(ReminderService));
        }

        public async Task OnReadyAsync(EspeonDbContext context) {
            this._logger.Information("Loading all reminders");
            var reminders = await context.GetRemindersAsync();
            foreach (var reminder in reminders) {
                if (reminder.TriggerAt < DateTimeOffset.Now) {
                    this._logger.Debug("Sending missed reminder for {User}", reminder.UserId);
                    await OnReminderAync(context, reminder, true);
                } else {
                    this._logger.Debug("Scheduling reminder for {User} at {At}", reminder.UserId, reminder.TriggerAt);
                    this._scheduler.DoAt(reminder.TriggerAt, (reminder, this._services), async state => { 
                        using var scope = this._services.CreateScope();
                        await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
                        await OnReminderAync(context, state.reminder, false);
                    });
                }
            }
        }
        
        public async Task CreateReminderAsync(UserReminder reminder) {
            this._logger.Debug("Creating reminder for {User}", reminder.UserId);
            using var scope = this._services.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            await context.PersistAsync(reminder);
            this._scheduler.DoAt(reminder.TriggerAt, (reminder, this._services), async state => {
                using var scope = state._services.CreateScope();
                await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
                await OnReminderAync(context, state.reminder, false);
            });
        }
        
        private async Task OnReminderAync(EspeonDbContext context, UserReminder reminder, bool late) {
            if (this._espeon.GetChannel(reminder.ChannelId) is CachedTextChannel channel
                    && channel.Guild.GetMember(reminder.UserId) is { }) {
                this._logger.Debug("Sending reminder for {User}", reminder.UserId);
                var originalMessage = await channel.GetMessageAsync(reminder.ReminderMessageId);
                var embed = new LocalEmbedBuilder()
                    .WithColor(Constants.EspeonColour)
                    .WithDescription(reminder.Value)
                    .WithTitle(late ? "A (Late) Reminder" : "A Reminder")
                    .AddField(ZeroWidthCharacter, Markdown.Link("Original Message", originalMessage?.GetJumpUrl(channel.Guild)))
                    .WithFooter("Created")
                    .WithTimestamp(originalMessage?.CreatedAt)
                    .Build();
;                await channel.SendMessageAsync($"<@{reminder.UserId}>", embed: embed);
            }

            await context.RemoveAsync(reminder);
        }
    }
}