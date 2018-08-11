using System;
using LiteDB;
using Umbreon.Interfaces;

namespace Umbreon.Core.Models.Database.Guilds
{
    public class Reminder : IRemoveable
    {
        [BsonId(true)]
        public int Id { get; set; }

        public string TheReminder { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public DateTime ToExecute { get; set; }

        public int Identifier { get; set; }
        public TimeSpan When { get; set; }
        public IRemoveableService Service { get; set; }
    }
}
