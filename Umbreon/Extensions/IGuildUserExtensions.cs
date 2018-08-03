using Discord;
using Discord.Commands;
using System;
using System.Linq;

namespace Umbreon.Extensions
{
    public static class IGuildUserExtensions
    {
        public static string GetDisplayName(this IGuildUser guildUser)
            => guildUser.Nickname ?? guildUser.Username;

        public static bool HasRole(this IGuildUser guildUser, ICommandContext context, ulong roleId)
            => HasRole(guildUser, context, context.Guild.GetRole(roleId));

        public static bool HasRole(this IGuildUser guildUser, ICommandContext context, IRole role)
            => HasRole(guildUser, context, role.Name);

        public static bool HasRole(this IGuildUser guildUser, ICommandContext context, string roleName)
            => guildUser.RoleIds.Select(x => context.Guild.GetRole(x).Name).Contains(roleName, StringComparer.CurrentCultureIgnoreCase);

        public static string GetAvatarOrDefaultUrl(this IUser guildUser, ImageFormat format = ImageFormat.Auto)
            => guildUser.GetAvatarUrl(format) ?? guildUser.GetDefaultAvatarUrl();
    }
}
