using Discord;
using Discord.WebSocket;
using System.Linq;

namespace Umbreon.Extensions
{
    public static class SocketGuildExtensions
    {
        public static SocketTextChannel GetDefaultChannel(this SocketGuild guild)
            => GetDefaultChannel(guild, guild.CurrentUser);

        public static SocketTextChannel GetDefaultChannel(this SocketGuild guild, IGuildUser user)
            => guild.TextChannels.Where(x => user.GetPermissions(x).SendMessages && user.GetPermissions(x).ViewChannel)
                .OrderBy(x => x.Position).FirstOrDefault();
    }
}
