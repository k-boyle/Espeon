using System.Collections.Generic;
using Espeon.Core.Entities;
using LiteDB;

namespace Espeon.Entities
{
    public class User : DatabaseEntity
    {
        public User() { }

        [BsonId(false)]
        public override ulong Id { get; set; }
        
        public override long WhenToRemove { get; set; }

        public IList<Reminder> Reminders { get; set; } = new List<Reminder>();

        public CandyData Candies { get; set; }
    }

    public class Reminder : BaseReminder, IRemovable
    {
        public override string TheReminder { get; set; }
        public override string JumpUrl { get; set; }
        public override ulong GuildId { get; set; }
        public override ulong ChannelId { get; set; }
        public override ulong UserId { get; set; }
        public override int Id { get; set; }

        public string TaskKey { get; set; }
        public long WhenToRemove { get; set; }
    }

    public class CandyData
    {
        public int Amount { get; set; }
        public int Highest { get; set;  }
        public long LastClaimed { get; set; }
    }
}
