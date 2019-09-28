using System;

namespace Espeon.Bot.Entities
{
    public struct CachedMessage
    {
        public ulong ChannelId { get; }
        public ulong ExecutingId { get; }
        public ulong UserId { get; }
        public ulong ResponseId { get; }

        public bool HasAttachment { get; }
        public bool IsPaginated { get; }

        public DateTimeOffset CreatedAt { get;}

        public CachedMessage(ulong channelId, ulong executingId, ulong userId, ulong responseId,
            bool hasAttachment, bool isPaginated, DateTimeOffset createdAt)
        {
            ChannelId = channelId;
            ExecutingId = executingId;
            UserId = userId;
            ResponseId = responseId;
            HasAttachment = hasAttachment;
            IsPaginated = isPaginated;
            CreatedAt = createdAt;
        }
    }
}
