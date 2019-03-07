using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using Espeon.Interactive.Criteria;

namespace Espeon.Interactive
{
    public interface IReactionCallback
    {
        EspeonContext Context { get; }

        IUserMessage Message { get; }
        IEnumerable<IEmote> Reactions { get; }
        ICriterion<SocketReaction> Criterion { get; }
            
        Task InitialiseAsync();
        Task HandleTimeoutAsync();
        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}
