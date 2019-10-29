using Espeon.Core.Database.CommandStore;
using Espeon.Core.Database.GuildStore;
using Espeon.Core.Database.UserStore;

namespace Espeon.Services {
	public struct InitialiseArgs {
		public UserStore UserStore { get; set; }
		public GuildStore GuildStore { get; set; }
		public CommandStore CommandStore { get; set; }
	}
}