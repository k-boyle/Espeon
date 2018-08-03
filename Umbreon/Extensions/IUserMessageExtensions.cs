using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Umbreon.Extensions
{
    public static class IUserMessageExtensions
    {
        //should be removed *soon*
        public static string GetJumpLink(this IUserMessage message) => $"https://discordapp.com/channels/{(message.Channel as IGuildChannel).Guild.Id}/{message.Channel.Id}/{message.Id}";

        public static async Task AddReactionsAsync(this IUserMessage msg, IEnumerable<IEmote> emotes)
        {
            foreach (var emote in emotes)
            {
                await msg.AddReactionAsync(emote);
            }
        }
    }
}
