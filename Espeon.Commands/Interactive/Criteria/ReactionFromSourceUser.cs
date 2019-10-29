
using Disqord.Events;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class ReactionFromSourceUser : ICriterion<ReactionAddedEventArgs> {
		private readonly ulong _userId;

		public ReactionFromSourceUser(ulong userId) {
			this._userId = userId;
		}

		public ValueTask<bool> JudgeAsync(EspeonContext context, ReactionAddedEventArgs reaction) {
			return new ValueTask<bool>(reaction.User.Id == this._userId);
		}
	}
}