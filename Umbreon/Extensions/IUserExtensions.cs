using Discord;
using Discord.Net;
using System.Threading.Tasks;

namespace Umbreon.Extensions
{
    public static class IUserExtensions
    {
        public static async Task<IUserMessage> TrySendDMAsync(this IUser user, string content, bool isTTS = false, Embed embed = null)
        {
            try
            {
                return await user.SendMessageAsync(content, isTTS, embed);
            }
            catch (HttpException)
            {
                return null;
            }
        }
    }
}
