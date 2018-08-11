using System.Collections.Concurrent;
using SharpLink;

namespace Umbreon.Core.Entities
{
    public class LavalinkObject
    {
        public LavalinkPlayer Player { get; set; }
        public bool IsPaused { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public ConcurrentQueue<LavalinkTrack> Queue { get; set; }
    }
}
