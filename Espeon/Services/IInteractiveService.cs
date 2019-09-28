using Discord.WebSocket;
using Espeon.Commands;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public interface IInteractiveService
    {
        Task<SocketUserMessage> NextMessageAsync(EspeonContext context,
            ICriterion<SocketUserMessage> criterion, TimeSpan? timeout = null);

        Task<bool> TryAddCallbackAsync(IReactionCallback callback, TimeSpan? timeout = null);

        bool TryRemoveCallback(IReactionCallback callback);
    }
}
