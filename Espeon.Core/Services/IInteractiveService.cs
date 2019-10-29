using Disqord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface IInteractiveService<in TCallback, in TContext> where TContext : CommandContext {
		Task<CachedUserMessage> NextMessageAsync(TContext context, Func<CachedUserMessage, ValueTask<bool>> predicate,
			TimeSpan? timeout = null);

		Task<bool> TryAddCallbackAsync(TCallback callback, TimeSpan? timeout = null);

		bool TryRemoveCallback(TCallback callback);
	}
}