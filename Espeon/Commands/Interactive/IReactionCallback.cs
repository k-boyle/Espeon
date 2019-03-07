using Discord;
using Discord.WebSocket;
using Espeon.Commands.Interactive.Criteria;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands.Interactive
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
