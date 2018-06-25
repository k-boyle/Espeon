using System;

namespace Umbreon.Core.Extensions
{
    public class MessageModel
    {
        public ulong ExecutingMessageId { get; }
        public ulong UserId { get; }
        public ulong ChannelId { get; }
        public ulong MessageId { get; }
        public DateTimeOffset CreatedAt { get; }

        public MessageModel(ulong executingMessageId, ulong userId, ulong channelId, ulong messageId, DateTimeOffset createdAt)
        {
            ExecutingMessageId = executingMessageId;
            UserId = userId;
            ChannelId = channelId;
            MessageId = messageId;
            CreatedAt = createdAt;
        }
    }
}
