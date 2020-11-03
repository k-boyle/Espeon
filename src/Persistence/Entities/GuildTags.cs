using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espeon {
    [Table("guild_tags")]
    public class GuildTags {
        [Key]
        [Column("guild_id")]
        public ulong GuildId { get; set; }

        [Required]
        [Column("tags")]
        [InverseProperty("GuildTags")]
        public HashSet<GuildTag> Values { get; set; }

        public GuildTags(ulong guildId) {
            GuildId = guildId;
            Values = new HashSet<GuildTag>();
        }
    }
}