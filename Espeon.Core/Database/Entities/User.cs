using System.Collections.Generic;

namespace Espeon.Core.Database {
	public class User {
		public ulong Id { get; set; }

		public ResponsePack ResponsePack { get; set; }

		public List<Reminder> Reminders { get; set; }

		public List<ResponsePack> ResponsePacks { get; set; }

		public List<DelayedCommand> Commands { get; set; }

		public int CandyAmount { get; set; }
		public int HighestCandies { get; set; }
		public long LastClaimedCandies { get; set; }

		public bool BoughtEmotes { get; set; }
	}
}