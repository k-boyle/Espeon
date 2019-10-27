using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class DeleteCallback : IReactionCallback {
		public EspeonContext Context { get; }

		public bool RunOnGatewayThread => true;

		public IUserMessage Message { get; }
		public ICriterion<SocketReaction> Criterion { get; }

		private readonly IEmote _deleteEmote;

		public IEnumerable<IEmote> Reactions =>
			new[] {
				this._deleteEmote
			};

		private bool _isDeleted;

		private readonly SemaphoreSlim _deleteSemaphore;

		public DeleteCallback(EspeonContext context, IUserMessage message, IEmote deleteEmote,
			ICriterion<SocketReaction> criterion = null) {
			Context = context;
			Message = message;

			Criterion = criterion ?? new ReactionFromSourceUser(context.User.Id);

			this._deleteEmote = deleteEmote;

			this._deleteSemaphore = new SemaphoreSlim(1);
		}

		public Task InitialiseAsync() {
			return Task.CompletedTask;
		}

		public Task HandleTimeoutAsync() {
			return this._isDeleted ? Task.CompletedTask : Message.DeleteAsync();
		}

		public async Task<bool> HandleCallbackAsync(SocketReaction reaction) {
			await this._deleteSemaphore.WaitAsync();

			if (!reaction.Emote.Equals(this._deleteEmote)) {
				this._deleteSemaphore.Release();
				return false;
			}

			await Message.DeleteAsync();
			this._isDeleted = true;

			this._deleteSemaphore.Release();
			return true;
		}
	}
}