using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;

namespace Espeon.Services
{
    public class InitialiseArgs
    {
        public UserStore UserStore { get; set; }
        public GuildStore GuildStore { get; set; }
        public CommandStore CommandStore { get; set; }
    }
}
