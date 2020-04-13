using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espeon {
    [Table("reminders")]
    public class UserReminder {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        [Required]
        [Column("user_id")]
        public ulong UserId { get; set; }

        [Required]
        [Column("message_id")]
        public ulong ReminderMessageId { get; set; }

        [Required]
        [Column("trigger_at")]
        public DateTimeOffset TriggerAt { get; set; }

        [Required]
        [Column("reminder_string")]
        public string Value { get; set; }

        public UserReminder(ulong channelId, ulong userId, ulong reminderMessageId, DateTimeOffset triggerAt, string value) {
            ChannelId = channelId;
            UserId = userId;
            ReminderMessageId = reminderMessageId;
            TriggerAt = triggerAt;
            Value = value;
        }
    }
}