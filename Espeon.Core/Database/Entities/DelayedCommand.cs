using System;

namespace Espeon.Core.Database {
	public class DelayedCommand {
		public string Id { get; set; }

		public ulong UserId { get; set; }
		public ulong ChannelId { get; set; }

		public DateTimeOffset When { get; set; }

		public string Command { get; set; }
	}
}