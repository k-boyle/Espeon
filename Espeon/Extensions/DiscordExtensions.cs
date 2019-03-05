using Discord;

namespace Espeon.Extensions
{
    public static partial class Extensions
    {
        public static string GetAvatarOrDefaultUrl(this IUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }

        public static string GetDisplayName(this IGuildUser user)
        {
            return user.Nickname ?? user.Username;
        }
    }
}
