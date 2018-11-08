using Espeon.Core.Entities;
using System.Collections.Generic;

namespace Espeon.Implementation.Entities
{
    public class Message : IRemovable
    {
        public ulong ExecutingId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public IList<ulong> ResponseIds { get; set; }

        public long WhenToRemove { get; set; }
    }
}
