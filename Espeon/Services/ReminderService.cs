using Casino.Common;
using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Databases.UserStore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Reminder = Espeon.Databases.Reminder;

namespace Espeon.Services
{
    public class ReminderService : BaseService
    {
        [Inject] private readonly LogService _logger;
        [Inject] private readonly TaskQueue _scheduler;
        [Inject] private readonly IServiceProvider _services;
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        public ReminderService(IServiceProvider services) : base(services)
        {
        }
        
        //requires client to be populated to send reminders
        public async Task LoadRemindersAsync(UserStore ctx)
        {
            await _logger.LogAsync(Source.Reminders, Severity.Info, "Sending all missed reminders");

            var toRemove = new List<Reminder>();

            var reminders = ctx.Reminders;
            foreach (var reminder in reminders)
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > reminder.WhenToRemove)
                {
                    toRemove.Add(reminder);
                    continue;
                }

                var newKey = _scheduler.ScheduleTask(reminder, reminder.WhenToRemove, RemoveAsync);
                reminder.TaskKey = newKey;
            }

            await Task.WhenAll(toRemove.Select(x => RemoveAsync(x.TaskKey, x)));
        }

        public async Task<Reminder> CreateReminderAsync(EspeonContext context, string content, TimeSpan when)
        {
            var reminder = new Reminder
            {
                ChannelId = context.Channel.Id,
                GuildId = context.Guild.Id,
                JumpUrl = context.Message.GetJumpUrl(),
                TheReminder = content,
                UserId = context.User.Id,
                WhenToRemove = DateTimeOffset.UtcNow.Add(when).ToUnixTimeMilliseconds(),
                ReminderId = Random.Next(999)
            };

            var key = _scheduler.ScheduleTask(reminder, reminder.WhenToRemove, RemoveAsync);

            reminder.TaskKey = key;

            await context.UserStore.Reminders.AddAsync(reminder);
            await context.UserStore.SaveChangesAsync();

            return reminder;
        }

        private async Task CancelReminderAsync(EspeonContext context, Reminder reminder)
        {
            _scheduler.CancelTask(reminder.TaskKey);

            context.UserStore.Remove(reminder);
            await context.UserStore.SaveChangesAsync();
        }

        public async Task<ImmutableArray<Reminder>> GetRemindersAsync(EspeonContext context)
        {
            var user = await context.UserStore.GetOrCreateUserAsync(context.User, x => x.Reminders);
            return user.Reminders.ToImmutableArray();
        }

        private async Task RemoveAsync(Guid taskKey, object removable)
        {
            var reminder = (Reminder)removable;

            if (!(_client.GetGuild(reminder.GuildId) is SocketGuild guild))
                return;

            if (!(_client.GetChannel(reminder.ChannelId) is SocketTextChannel channel))
                return;

            if (!(guild.GetUser(reminder.UserId) is IGuildUser user))
                return;

            var embed = ResponseBuilder.Reminder(user, ReminderString(reminder.TheReminder, reminder.JumpUrl));

            await channel.SendMessageAsync(user.Mention, embed: embed);

            var ctx = _services.GetService<UserStore>();

            ctx.Reminders.Remove(reminder);
            await ctx.SaveChangesAsync();

            await _logger.LogAsync(Source.Reminders, Severity.Verbose,
                $"Sent reminder for {{{user.GetDisplayName()}}} in {{{guild.Name}}}/{{{channel.Name}}}");
        }

        private static string ReminderString(string reminder, string jumpUrl)
        {
            return $"{reminder}\n\n[Original Message]({jumpUrl})";
        }
    }
}
