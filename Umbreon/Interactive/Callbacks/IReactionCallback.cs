using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Umbreon.Interactive.Callbacks
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        Criteria.ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        ICommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}
