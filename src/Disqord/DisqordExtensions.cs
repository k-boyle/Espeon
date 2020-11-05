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
        
        public static ValueTask<IMember> GetOrFetchMemberAsync(this CachedGuild guild, Snowflake memberId) {
            static async Task<IMember> GetMemberAsync(CachedGuild guild, Snowflake memberId) {
                return await guild.GetMemberAsync(memberId);
            }
            
            return guild.GetMember(memberId) is { } member
                ? new ValueTask<IMember>(member)
                : new ValueTask<IMember>(GetMemberAsync(guild, memberId));
        }
    }
}