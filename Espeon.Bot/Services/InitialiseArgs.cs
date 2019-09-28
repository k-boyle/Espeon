using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;

namespace Espeon.Bot.Services
{
    public struct InitialiseArgs
    {
        public UserStore UserStore { get; set; }
        public GuildStore GuildStore { get; set; }
        public CommandStore CommandStore { get; set; }
    }
}
