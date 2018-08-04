using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

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
