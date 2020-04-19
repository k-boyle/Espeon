using Disqord;

namespace Espeon {
    public static class DisqordExtensions {
        public static string GetJumpUrl(this IMessage message, IGuild guild = null) {
            return $"https://discordapp.com/channels/{guild?.Id.RawValue.ToString() ?? "@me"}/{message.ChannelId}/{message.Id}";
        }
    }
}