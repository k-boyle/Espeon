using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Core.Entities.Guild;
using Espeon.Extensions;
using Espeon.Interfaces;
using Colour = Discord.Color;

namespace Espeon.Services
{
    [Service]
    public class RemindersService : IRemoveableService
    {
        private readonly DatabaseService _database;
        private readonly TimerService _timer;
        private readonly DiscordSocketClient _client;
        private readonly MessageService _message;
        private readonly Random _random;
        
        public RemindersService() { }

        public RemindersService(DatabaseService database, TimerService timer, DiscordSocketClient client, MessageService message, Random random)
        {
            _database = database;
            _timer = timer;
            _client = client;
            _message = message;
            _random = random;
        }

        public async Task LoadRemindersAsync()
        {
            var toRemove = new List<Reminder>();

            foreach (var guild in _client.Guilds)
            {
                var reminders = _database.TempLoad<GuildObject>("guilds", guild.Id).Reminders;
                foreach (var reminder in reminders)
                {
                    var initializedReminder = new Reminder(reminder, this);
                    if (initializedReminder.When.ToUniversalTime() < DateTime.UtcNow)
                    {
                        toRemove.Add(initializedReminder);
                        continue;
                    }

                    await _timer.EnqueueAsync(initializedReminder);
                }
            }

            foreach (var reminder in toRemove)
                await RemoveAsync(reminder);
        }

        public async Task RemoveAsync(IRemoveable obj)
        {
            if (!(obj is Reminder reminder)) return;
            var user = _client.GetGuild(reminder.GuildId).GetUser(reminder.UserId);
            await _message.NewMessageAsync(reminder.UserId, 0, reminder.ChannelId, $"{user.Mention}", embed: new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatarOrDefaultUrl(),
                    Name = user.GetDisplayName()
                },
                Color = Colour.DarkBlue,
                Description = reminder.TheReminder
            }.Build());
            var guild = await _database.GetObjectAsync<GuildObject>("guilds", reminder.GuildId);
            guild.Reminders.Remove(guild.Reminders.Find(x => x.Identifier == reminder.Identifier));
            _database.UpdateObject("guilds", guild);
        }

        public async Task CreateReminderAsync(string content, ulong guildId, ulong channelId, ulong userId, TimeSpan toExecute)
        {
            var reminder = new Reminder
            {
                ChannelId = channelId,
                GuildId = guildId,
                TheReminder = content,
                UserId = userId,
                When = DateTime.UtcNow + toExecute,
                Identifier = _random.Next()
            };

            await _timer.EnqueueAsync(reminder);

            var guild = await _database.GetObjectAsync<GuildObject>("guilds", guildId);
            guild.Reminders.Add(reminder);
            _database.UpdateObject("guilds", guild);
        }
    }
}
