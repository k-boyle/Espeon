using Disqord;
using System.Threading.Tasks;

namespace Espeon {
    public static class DisqordExtensions {
        public static string GetJumpUrl(this IMessage message, IGuild guild = null) {
            return $"https://discordapp.com/channels/{guild?.Id.RawValue.ToString() ?? "@me"}/{message.ChannelId}/{message.Id}";
        }
        
        public static ValueTask<IMessage> GetOrFetchMessageAsync(this ICachedMessageChannel channel, ulong messageId) {
            static async Task<IMessage> GetMessageAsync(IMessageChannel channel, ulong messageId) {
                return await channel.GetMessageAsync(messageId);
            }

            return channel.GetMessage(messageId) is { } message
                ? new ValueTask<IMessage>(message)
                : new ValueTask<IMessage>(GetMessageAsync(channel, messageId));
        }
    }
}