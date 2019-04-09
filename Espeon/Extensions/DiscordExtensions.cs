using Discord;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    public static partial class Extensions
    {
        private static DiscordSocketRestClient RestClient { get; set; }

        public static string GetAvatarOrDefaultUrl(this IUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }

        public static string GetDisplayName(this IGuildUser user)
        {
            return user.Nickname ?? user.Username;
        }

        public static async Task<IGuildUser> GetGuildUserAsync(this SocketGuild guild, ulong userId)
        {
            var user = guild.GetUser(userId) as IGuildUser;

            if(user is null)
            {
                if (RestClient is null)
                {
                    var type = guild.GetType();
                    var prop = type.GetProperty("Discord", BindingFlags.Instance | BindingFlags.NonPublic);

                    var client = (DiscordSocketClient)prop.GetValue(guild);
                    RestClient = client.Rest;
                }

                user = await RestClient.GetGuildUserAsync(guild.Id, userId);
            }

            return user;
        }

        public static async Task<IUser> GetUserAsync(this DiscordSocketClient client, ulong userId)
        {
            return client.GetUser(userId) ?? await client.Rest.GetUserAsync(userId) as IUser;
        }
    }
}
