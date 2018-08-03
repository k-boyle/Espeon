using System;

namespace Umbreon.Core.Entities
{
    public class Message
    {
        public ulong ExecutingId { get; set; }
        public ulong ResponseId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
