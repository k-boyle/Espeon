using Espeon.Entities;
using System.Collections.Generic;

namespace Espeon.Database.Entities
{
    public class User : DatabaseEntity
    {
        public override ulong Id { get; set; }

        public override long WhenToRemove { get; set; }

        public string ResponsePack { get; set; } = "default";

        public List<Reminder> Reminders { get; set; } = new List<Reminder>();

        public CandyData Candies { get; set; } = new CandyData();
    }

    public class Reminder : IRemovable
    {
        public string Id { get; set; }

        public string TheReminder { get; set; }
        public string JumpUrl { get; set; }
        
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public int ReminderId { get; set; }

        public string TaskKey { get; set; }
        public long WhenToRemove { get; set; }
    }

    public class CandyData
    {
        public User User { get; set; }
        public ulong UserId { get; set; }

        public int Amount { get; set; }
        public int Highest { get; set;  }
        public long LastClaimed { get; set; }
    }
}
