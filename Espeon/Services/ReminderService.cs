using Discord;
using Discord.WebSocket;
using Espeon.Attributes;
using Espeon.Commands;
using Espeon.Entities;
using Microsoft.Extensions.DependencyInjection;
using Pusharp;
using Pusharp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(ServiceLifetime.Singleton)]
    public class ReminderService
    {
        [Inject] private readonly DatabaseService _database;
        [Inject] private readonly LogService _logger;
        [Inject] private readonly TimerService _timer;

        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly PushBulletClient _push;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        private Device _phone;
        private Device Phone => _phone ?? (_phone = _push.Devices.First());

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
                    
                    await _database.WriteEntityAsync("users", user);
                }
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
                Id = Random.Next(999)
            };

            var key = await _timer.EnqueueAsync(reminder, RemoveAsync);

            reminder.TaskKey = key;

            var user = await _database.GetEntityAsync<User>("users", context.User.Id) ?? new User
            {
                Id = context.User.Id
            };

            user.Reminders.Add(reminder);
            await _database.WriteEntityAsync("users", user);

            return reminder;
        }
        
        private async Task CancelReminderAsync(EspeonContext context, Reminder reminder)
        {
            await _timer.RemoveAsync(reminder.TaskKey);

            var user = await _database.GetEntityAsync<User>("users", reminder.UserId);
            user.Reminders = user.Reminders.Where(x => x.TaskKey != reminder.TaskKey).ToList();

            await _database.WriteEntityAsync("users", user);
        }
        
        public async Task<ImmutableArray<Reminder>> GetRemindersAsync(EspeonContext context)
        {
            var user = await _database.GetEntityAsync<User>("users", context.User.Id);

            return user.Reminders.Cast<Reminder>().ToImmutableArray();
        }

        private async Task RemoveAsync(string taskKey, IRemovable removable)
        {
            var reminder = removable as Reminder;

            var appInfo = await _client.GetApplicationInfoAsync();

            if (reminder.UserId == appInfo.Owner.Id)
            {
                await Phone.SendNoteAsync(x =>
                {
                    x.Title = "Reminder!";
                    x.Body = reminder.TheReminder;
                });
            }

            if (!(_client.GetGuild(reminder.GuildId) is SocketGuild guild))
                return;

            if (!(_client.GetChannel(reminder.ChannelId) is SocketTextChannel channel))
                return;

            if (!(guild.GetUser(reminder.UserId) is IGuildUser user))
                return;

            var embed = ResponseBuilder.Reminder(user, ReminderString(reminder.TheReminder, reminder.JumpUrl));

            await channel.SendMessageAsync(user.Mention, embed: embed);

            var dUser = await _database.GetEntityAsync<User>("users", user.Id);
            dUser.Reminders = dUser.Reminders.Where(x => x.TaskKey != taskKey).ToList();

            await _database.WriteEntityAsync("users", dUser);

            await _logger.LogAsync(Source.Reminders, Severity.Verbose,
                $"Executed reminder for {{{user.GetDisplayName()}}} in {{{guild.Name}}}/{{{channel.Name}}}");
        }

        private static string ReminderString(string reminder, string jumpUrl)
        {
            return $"{reminder}\n\n[Original Message]({jumpUrl})";
        }
    }
}
