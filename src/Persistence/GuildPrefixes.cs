using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espeon.Persistence {
    [Table("prefixes")]
    public class GuildPrefixes {
        [Key]
        [Column("guild_id")]
        public ulong GuildId { get; set; }
        
        [Required]
        [Column("prefixes")]
        public string[] Prefixes { get; set; }

        public GuildPrefixes(ulong guildId) {
            GuildId = guildId;
            Prefixes = new[] {
                "es/"
            };
        }
    }
}