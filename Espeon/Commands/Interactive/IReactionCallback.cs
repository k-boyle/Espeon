using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public interface IReactionCallback
    {
        EspeonContext Context { get; }

        bool RunOnGatewayThread { get; }

        IUserMessage Message { get; }
        IEnumerable<IEmote> Reactions { get; }
        ICriterion<SocketReaction> Criterion { get; }

        Task InitialiseAsync();
        Task HandleTimeoutAsync();
        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}
