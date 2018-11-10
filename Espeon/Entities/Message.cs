using Espeon.Core.Entities;

namespace Espeon.Entities
{
    public class CachedMessage : IRemovable
    {
        public ulong ExecutingId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public ulong ResponseId { get; set; }
        
        public long WhenToRemove { get; set; }
    }
}
