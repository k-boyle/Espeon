using System.Collections.Generic;

namespace Espeon.Databases
{
    public class StarredMessage
    {
        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        public ulong Id { get; set; }
        public ulong ChannelId { get; set; }
        public ulong AuthorId { get; set; }
        public ulong StarboardMessageId { get; set; }

        public ICollection<ulong> ReactionUsers { get; set; }

        public string ImageUrl { get; set; }
        public string Content { get; set; }
    }
}
