using Discord;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Umbreon.Extensions
{
    public static class IUserMessageExtensions
    {
        //should be removed *soon*
        public static string GetJumpLink(this IUserMessage message) => $"https://discordapp.com/channels/{(message.Channel as IGuildChannel).Guild.Id}/{message.Channel.Id}/{message.Id}";

        public static Task AddReactionsAsync(this IUserMessage msg, RequestOptions options = null, params IEmote[] emotes)
            => AddReactionsAsync(msg, emotes.ToImmutableArray(), options);

        public static Task AddReactionsAsync(this IUserMessage msg, IEnumerable<IEmote> emotes, RequestOptions options = null)
            => Task.WhenAll(emotes.Select(x => msg.AddReactionAsync(x, options ?? RequestOptions.Default)));
    }
}
