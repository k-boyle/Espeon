using System;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core.Models.Database.Guilds;
using Umbreon.Interfaces;

namespace Umbreon.Services
{
    [Service]
    public class RemindersService : IRemoveableService
    {
        private readonly DatabaseService _database;
        private readonly TimerService _timer;
        private readonly DiscordSocketClient _client;
        private readonly MessageService _message;
        private readonly Random _random;

        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<Reminder>> _reminders = new ConcurrentDictionary<ulong, ConcurrentQueue<Reminder>>();
        
        public RemindersService() { }

        public RemindersService(DatabaseService database, TimerService timer, DiscordSocketClient client, MessageService message, Random random)
        {
            _database = database;
            _timer = timer;
            _client = client;
            _message = message;
            _random = random;
        }

        public async Task LoadReminders()
        {
            var toRemove = new List<Reminder>();

            foreach (var guild in _client.Guilds)
            {
                var reminders = _database.TempLoad(guild).Reminders;
                foreach (var reminder in reminders)
                {
                    if (reminder.When.ToUniversalTime() < DateTime.UtcNow)
                    {
                        toRemove.Add(reminder);
                        continue;
                    }

                    _timer.Enqueue(reminder);
                }
            }

            foreach (var reminder in toRemove)
                await Remove(reminder);
        }

        public async Task Remove(IRemoveable obj)
        {
            if (!(obj is Reminder reminder)) return;
            var user = _client.GetGuild(reminder.GuildId).GetUser(reminder.UserId);
            await _message.NewMessageAsync(reminder.UserId, 0, reminder.ChannelId, $"{user.Mention} you wanted me to remind you:\n{reminder.TheReminder}");
            var guild = _database.GetGuild(reminder.GuildId);
            guild.Reminders.RemoveAt(guild.Reminders.FindIndex(x => x.Id == reminder.Id));
            _database.UpdateGuild(guild);
        }

        public void CreateReminder(string content, ulong guildId, ulong channelId, ulong userId, TimeSpan toExecute)
        {
            var reminder = new Reminder
            {
                ChannelId = channelId,
                GuildId = guildId,
                TheReminder = content,
                Service = this,
                UserId = userId,
                When = DateTime.UtcNow + toExecute,
                Identifier = _random.Next()
            };

            _timer.Enqueue(reminder);

            var guild = _database.GetGuild(guildId);
            guild.Reminders.Add(reminder);
            _database.UpdateGuild(guild);
        }
    }
}
