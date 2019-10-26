using Espeon.Core.Databases.CommandStore;
using Espeon.Core.Databases.GuildStore;
using Espeon.Core.Databases.UserStore;

namespace Espeon.Services {
	public struct InitialiseArgs {
		public UserStore UserStore { get; set; }
		public GuildStore GuildStore { get; set; }
		public CommandStore CommandStore { get; set; }
	}
}