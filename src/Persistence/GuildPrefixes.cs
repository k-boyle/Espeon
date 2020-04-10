using Disqord.Bot.Prefixes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espeon {
    [Table("prefixes")]
    public class GuildPrefixes {
        [Key]
        [Column("guild_id")]
        public ulong GuildId { get; set; }
        
        [Required]
        [Column("prefixes")]
        public HashSet<IPrefix> Values { get; set; }

        public GuildPrefixes(ulong guildId) {
            GuildId = guildId;
            Values = new HashSet<IPrefix> {
                new StringPrefix("es/"),
                MentionPrefix.Instance
            };
        }
    }
}