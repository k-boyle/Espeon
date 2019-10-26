using System.Collections.Generic;

namespace Espeon.Core.Databases {
	public class Guild {
		public ulong Id { get; set; }

		public ulong WelcomeChannelId { get; set; }
		public string WelcomeMessage { get; set; }

		public ulong DefaultRoleId { get; set; }

		public int WarningLimit { get; set; }
		public List<Warning> Warnings { get; set; }

		public ulong NoReactions { get; set; }

		public List<string> Prefixes { get; set; }

		public ICollection<ulong> RestrictedChannels { get; set; }
		public ICollection<ulong> RestrictedUsers { get; set; }

		public ICollection<ulong> Admins { get; set; }
		public ICollection<ulong> Moderators { get; set; }

		public ICollection<ulong> SelfAssigningRoles { get; set; }

		public List<MockWebhook> Webhooks { get; set; }

		public List<CustomCommand> Commands { get; set; }

		public ulong StarboardChannelId { get; set; }
		public int StarLimit { get; set; }
		public List<StarredMessage> StarredMessages { get; set; }

		public bool EmotesEnabled { get; set; }

		public bool AutoQuotes { get; set; }
	}
}