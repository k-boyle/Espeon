using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espeon {
    [Table("tags")]
    public abstract class Tag {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        
        [Required]
        [Column("tag_key")]
        public string Key { get; set; }

        [Required]
        [Column("tag_string")]
        public string Value { get; set; }

        [Required]
        [Column("created_at")]
        public DateTimeOffset CreateAt { get; set; }

        [Column("uses")]
        public int Uses { get; set; }

        protected Tag(string key, string value) {
            Key = key;
            Value = value;
            CreateAt = DateTimeOffset.Now;
        }
    }
}