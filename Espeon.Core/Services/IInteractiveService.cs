using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface IInteractiveService<in TCallback, in TContext> where TContext : CommandContext {
		Task<SocketUserMessage> NextMessageAsync(TContext context, Func<SocketUserMessage, ValueTask<bool>> predicate,
			TimeSpan? timeout = null);

		Task<bool> TryAddCallbackAsync(TCallback callback, TimeSpan? timeout = null);

		bool TryRemoveCallback(TCallback callback);
	}
}