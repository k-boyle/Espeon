using System;

namespace Espeon.Core.Databases {
	public class Reminder {
		public string Id { get; set; }

		public string TheReminder { get; set; }
		public string JumpUrl { get; set; }

		public ulong UserId { get; set; }
		public ulong GuildId { get; set; }
		public ulong ChannelId { get; set; }
		public ulong InvokeId { get; set; }
		public int ReminderId { get; set; }

		public DateTimeOffset WhenToRemove { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
	}
}