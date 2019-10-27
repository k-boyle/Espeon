using Discord.WebSocket;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class ReactionFromSourceUser : ICriterion<SocketReaction> {
		private readonly ulong _userId;

		public ReactionFromSourceUser(ulong userId) {
			this._userId = userId;
		}

		public ValueTask<bool> JudgeAsync(EspeonContext context, SocketReaction reaction) {
			return new ValueTask<bool>(reaction.UserId == this._userId);
		}
	}
}