using Discord.WebSocket;
using System.Linq;

namespace Espeon.Extensions
{
    public static class SocketGuildExtensions
    {
        public static SocketTextChannel GetDefaultChannel(this SocketGuild guild)
            => guild.TextChannels.Where(x => guild.CurrentUser.GetPermissions(x).SendMessages && guild.CurrentUser.GetPermissions(x).ViewChannel)
                .OrderBy(x => x.Position).FirstOrDefault();
    }
}
