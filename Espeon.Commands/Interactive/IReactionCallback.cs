using Disqord;
using Disqord.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public interface IReactionCallback {
		EspeonContext Context { get; }

		bool RunOnGatewayThread { get; }

		IUserMessage Message { get; }
		IEnumerable<IEmoji> Reactions { get; }
		ICriterion<ReactionAddedEventArgs> Criterion { get; }

		Task InitialiseAsync();
		Task HandleTimeoutAsync();
		Task<bool> HandleCallbackAsync(ReactionAddedEventArgs reaction);
	}
}