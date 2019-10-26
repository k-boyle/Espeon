using Discord.WebSocket;
using Espeon.Core.Commands;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class ReactionFromSourceUser : ICriterion<SocketReaction> {
		private readonly ulong _userId;

		public ReactionFromSourceUser(ulong userId) {
			this._userId = userId;
		}

		public Task<bool> JudgeAsync(EspeonContext context, SocketReaction reaction) {
			return Task.FromResult(reaction.UserId == this._userId);
		}
	}
}