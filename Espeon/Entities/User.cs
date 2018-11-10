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

    public class Reminder : IRemovable
    {
        public string TheReminder { get; set; }
        public string JumpUrl { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }

        public int TaskKey { get; set; }
        public long WhenToRemove { get; set; }
    }

    public class CandyData
    {
        public int Amount { get; set; }
        public long LastClaimed { get; set; }
    }
}
