using Disqord;
using Disqord.Bot;
using Disqord.Events;
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
        
        public static async Task WaitForReadyAsync(this DiscordBot bot) {
            var tcs = new TaskCompletionSource<bool>();
            Task OnReadyAsync(ReadyEventArgs _) {
                tcs.SetResult(true);
                return Task.CompletedTask;
            }

            bot.Ready += OnReadyAsync;
            await tcs.Task;
            bot.Ready -= OnReadyAsync;
        }
    }
}