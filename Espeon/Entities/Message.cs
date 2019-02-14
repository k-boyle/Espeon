namespace Espeon.Entities
{
    public class CachedMessage
    {
        public ulong ChannelId { get; set; }
        public long CreatedAt { get; set; }
        public ulong ExecutingId { get; set; }
        public ulong ResponseId { get; set; }
        public ulong UserId { get; set; }
    }
}
