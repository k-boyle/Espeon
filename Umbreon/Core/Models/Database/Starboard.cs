using System.Collections.Generic;

namespace Umbreon.Core.Models.Database
{
    public class Starboard
    {
        public bool Enabled { get; set; } = false;
        public ulong ChannelId { get; set; }
        public int StarLimit { get; set; } = 1;
        public List<StarredMessage> StarredMessages { get; set; } = new List<StarredMessage>();
    }
}
