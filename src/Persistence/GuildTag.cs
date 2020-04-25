using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espeon {
    public class GuildTag : Tag {
        [Required]
        [Column("guild_id")]
        public ulong GuildId { get; set; }
        
        [Required]
        [Column("creator_id")]
        public ulong CreatorId { get; set; }
        
        [Required]
        [Column("owner_id")]
        public ulong OwnerId { get; set; }

        public GuildTag(string key, string value, ulong guildId, ulong creatorId) : base(key, value) {
            GuildId = guildId;
            CreatorId = creatorId;
            OwnerId = creatorId;
        }
    }
}