using System;
using LiteDB;

namespace Umbreon.Core.Models.Database.Guilds
{
    public class Warning
    {
        [BsonId(true)]
        public int Key { get; set; }

        public ulong WarnedUser { get; set; }
        public string Reason { get; set; }
        public DateTime IssuedAt { get; set; }
        public ulong ResponsibleUser { get; set; }
    }
}
