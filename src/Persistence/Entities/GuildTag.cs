using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espeon {
    public class GuildTag : Tag {
        [ForeignKey("GuildId")]
        [InverseProperty("Values")]
        public GuildTags GuildTags { get; set; }
        
        [Required]
        [Column("guild_id")]
        public ulong GuildId { get; set; }
        
        [Required]
        [Column("creator_id")]
        public ulong CreatorId { get; set; }
        
        [Required]
        [Column("owner_id")]
        public ulong OwnerId { get; set; }

        public GuildTag(ulong guildId, string key, string value, ulong creatorId) : base(key, value) {
            GuildId = guildId;
            CreatorId = creatorId;
            OwnerId = creatorId;
        }

        public override int GetHashCode() {
            return (CreatorId.GetHashCode() * 17 + OwnerId.GetHashCode()) * 17 + Key.ToLower().GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is GuildTag tag && tag.Key.Equals(Key, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}