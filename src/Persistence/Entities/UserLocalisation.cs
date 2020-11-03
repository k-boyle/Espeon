using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espeon {
    [Table("localisation")]
    public class UserLocalisation {
        [Column("guild_id")]
        public ulong GuildId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Required]
        [Column("localisation")]
        public Language Value { get; set; }

        public UserLocalisation(ulong guildId, ulong userId) {
            GuildId = guildId;
            UserId = userId;
            Value = Language.Default;
        }
    }
}