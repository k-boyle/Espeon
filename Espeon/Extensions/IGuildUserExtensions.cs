using Discord;
using System;
using System.Linq;

namespace Espeon.Extensions
{
    public static class IGuildUserExtensions
    {
        public static string GetDisplayName(this IGuildUser guildUser)
            => guildUser.Nickname ?? guildUser.Username;

        public static bool HasRole(this IGuildUser guildUser, ulong roleId)
            => HasRole(guildUser, guildUser.Guild.GetRole(roleId));

        public static bool HasRole(this IGuildUser guildUser, IRole role)
            => HasRole(guildUser, role.Name);

        public static bool HasRole(this IGuildUser guildUser, string roleName)
            => guildUser.RoleIds.Select(x => guildUser.Guild.GetRole(x).Name).Contains(roleName, StringComparer.CurrentCultureIgnoreCase);

        public static string GetAvatarOrDefaultUrl(this IUser guildUser, ImageFormat format = ImageFormat.Auto)
            => guildUser.GetAvatarUrl(format) ?? guildUser.GetDefaultAvatarUrl();
    }
}
