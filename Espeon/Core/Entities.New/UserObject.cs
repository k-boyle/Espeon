using Espeon.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Core.Entities.New
{
    public class UserObject : BaseObject
    {
        public UserObject(UserObject baseObj, IRemoveableService service) : base(baseObj, service)
        {
            Candies = baseObj.Candies;
        }

        public UserObject() { }

        public List<Reminder> Reminders { get; set; } = new List<Reminder>();

        public CandyData Candies { get; set; } = new CandyData();
    }

    public class Reminder : IRemoveable
    {
        private readonly IRemoveableService _service;

        public Reminder(Reminder reminder, IRemoveableService service)
        {
            TheReminder = reminder.TheReminder;
            JumpLink = reminder.JumpLink;
            GuildId = reminder.GuildId;
            ChannelId = reminder.ChannelId;
            UserId = reminder.UserId;
            Identifier = reminder.Identifier;
            When = reminder.When;
            _service = service;
        }

        public Reminder(IRemoveableService service)
        {
            _service = service;
        }

        public Reminder() { }

        public string TheReminder { get; set; }
        public string JumpLink { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }

        public int Identifier { get; set; }
        public DateTime When { get; set; }

        public Task RemoveAsync()
            => _service.RemoveAsync(this);
    }

    public class CandyData
    {
        public int Amount { get; set; } = 10;
        public DateTime LastClaimed { get; set; } = DateTime.UtcNow.AddHours(-8);
    }
}
