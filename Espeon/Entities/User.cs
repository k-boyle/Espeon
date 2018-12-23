using LiteDB;
using System.Collections.Generic;

namespace Espeon.Entities
{
    public class User : DatabaseEntity
    {
        public User() { }

        [BsonId(false)]
        public override ulong Id { get; set; }
        
        public override long WhenToRemove { get; set; }

        public string ResponsePack { get; set; } = "default";

        public IList<Reminder> Reminders { get; set; } = new List<Reminder>();

        public CandyData Candies { get; set; }
    }

    public class Reminder : IRemovable
    {
        public string TheReminder { get; set; }
        public string JumpUrl { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public int Id { get; set; }

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
