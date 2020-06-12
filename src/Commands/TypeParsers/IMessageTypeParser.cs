using Disqord;
using Qmmands;
using System;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    public class IMessageTypeParser : EspeonTypeParser<IMessage> {
        public override async ValueTask<TypeParserResult<IMessage>> ParseAsync(
                Parameter parameter,
                string value,
                EspeonCommandContext context) {
            if (ulong.TryParse(value, out var messageId)) {
                return await context.Channel.GetOrFetchMessageAsync(messageId) is { } message
                    ? TypeParserResult<IMessage>.Successful(message)
                    : new EspeonTypeParserFailedResult<IMessage>(INVALID_MESSAGE_ID_PATH);
            }
            
            if (TryParseJumpUrl(value, out var ids)) {
                if (context.Bot.GetChannel(ids.ChannelId) is ICachedMessageChannel channel) {
                    return await channel.GetOrFetchMessageAsync(ids.MessageId) is { } message
                        ? TypeParserResult<IMessage>.Successful(message)
                        : new EspeonTypeParserFailedResult<IMessage>(INVALID_MESSAGE_ID_PATH);
                }
            }
            
            return new EspeonTypeParserFailedResult<IMessage>(INVALID_MESSAGE_ID_PATH);
        }
        
        private static bool TryParseJumpUrl(string raw, out (ulong ChannelId, ulong MessageId) ids) {
            ids = default;
            var span = raw.AsSpan();
            var index = -1;
            return TryParseId(ref span, ref index, ref ids.MessageId)
                && TryParseId(ref span, ref index, ref ids.ChannelId);
        }
        
        private static bool TryParseId(ref ReadOnlySpan<char> span, ref int index, ref ulong id) {
            if (index != -1) {
                span = span.Slice(0, index);
            }
            index = span.LastIndexOf('/');
            return index != -1 && index < span.Length - 1 && ulong.TryParse(span.Slice(index + 1), out id);
        }
    }
}