using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Attributes;
using Espeon.Core.Commands;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using Espeon.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(typeof(IReminderService), true)]
    public class ReminderService : IReminderService
    {
        [Inject] private readonly IDatabaseService _database;
        [Inject] private readonly ILogService _logger;
        [Inject] private readonly ITimerService _timer;

        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        [Initialiser]
        public async Task LoadRemindersAsync()
        {
            var toRemove = new List<Reminder>();

            foreach (var user in await _database.GetCollectionAsync<User>("users"))
            {
                var reminders = user.Reminders;

                foreach (var reminder in reminders)
                {
                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > reminder.WhenToRemove)
                    {
                        toRemove.Add(reminder);
                        continue;
                    }

                    var newKey = await _timer.EnqueueAsync(reminder, RemoveAsync);
                    reminder.TaskKey = newKey;

                    //TODO check this
                    await _database.WriteAsync("users", user);
                }
            }

            await Task.WhenAll(toRemove.Select(x => RemoveAsync(x.TaskKey, x)));
        }

        async Task<BaseReminder> IReminderService.CreateReminderAsync(IEspeonContext context, string content, TimeSpan when)
            => await CreateReminderAsync(context, content, when);

        private async Task<Reminder> CreateReminderAsync(IEspeonContext context, string content, TimeSpan when)
        {
            var reminder = new Reminder
            {
                ChannelId = context.Channel.Id,
                GuildId = context.Guild.Id,
                JumpUrl = context.Message.GetJumpUrl(),
                TheReminder = content,
                UserId = context.User.Id,
                WhenToRemove = DateTimeOffset.UtcNow.Add(when).ToUnixTimeMilliseconds(),
                Id = Random.Next(999)
            };

            var key = await _timer.EnqueueAsync(reminder, RemoveAsync);

            reminder.TaskKey = key;

            var user = await _database.GetAndCacheEntityAsync<User>("users", context.User.Id);
            
            user.Reminders.Add(reminder);
            await _database.WriteAsync("users", user);

            return reminder;
        }

        Task IReminderService.CancelReminderAsync(IEspeonContext context, BaseReminder reminder)
            => CancelReminderAsync(context, reminder as Reminder);

        private async Task CancelReminderAsync(IEspeonContext context, Reminder reminder)
        {
            await _timer.RemoveAsync(reminder.TaskKey);

            var user = await _database.GetAndCacheEntityAsync<User>("users", reminder.UserId);
            user.Reminders = user.Reminders.Where(x => x.TaskKey != reminder.TaskKey).ToList();

            await _database.WriteAsync("users", user);
        }

        async Task<IReadOnlyCollection<BaseReminder>> IReminderService.GetRemindersAsync(IEspeonContext context)
            => await GetRemindersAsync(context);

        private async Task<IReadOnlyCollection<Reminder>> GetRemindersAsync(IEspeonContext context)
        {
            var user = await _database.GetAndCacheEntityAsync<User>("users", context.User.Id);

            return user.Reminders.ToImmutableList();
        }

        private async Task RemoveAsync(string taskKey, IRemovable removable)
        {
            var reminder = removable as Reminder;

            if (!(_client.GetGuild(reminder.GuildId) is SocketGuild guild))
                return;

            if (!(_client.GetChannel(reminder.ChannelId) is SocketTextChannel channel))
                return;

            if (!(guild.GetUser(reminder.UserId) is IGuildUser user))
                return;

            var embed = ResponseBuilder.Reminder(user, ReminderString(reminder.TheReminder, reminder.JumpUrl));

            await channel.SendMessageAsync(user.Mention, embed: embed);

            var dUser = await _database.GetAndCacheEntityAsync<User>("users", user.Id);
            dUser.Reminders = dUser.Reminders.Where(x => x.TaskKey != taskKey).ToList();

            await _database.WriteAsync("users", dUser);

            await _logger.LogAsync(new LogMessage(LogSeverity.Verbose, "Reminders",
                $"Executed reminder for {{{user.GetDisplayName()}}} in {{{guild.Name}}}/{{{channel.Name}}}"));
        }

        private static string ReminderString(string reminder, string jumpUrl)
        {
            return $"{reminder}\n\n[Original Message]({jumpUrl})";
        }
    }
}
