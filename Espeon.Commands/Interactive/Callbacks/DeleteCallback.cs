using Disqord;
using Disqord.Events;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class DeleteCallback : IReactionCallback {
		public EspeonContext Context { get; }

		public bool RunOnGatewayThread => true;

		public IUserMessage Message { get; }
		public ICriterion<ReactionAddedEventArgs> Criterion { get; }

		private readonly IEmoji _deleteEmote;

		public IEnumerable<IEmoji> Reactions =>
			new[] {
				this._deleteEmote
			};

		private bool _isDeleted;

		private readonly SemaphoreSlim _deleteSemaphore;

		public DeleteCallback(EspeonContext context, IUserMessage message, IEmoji deleteEmote,
			ICriterion<ReactionAddedEventArgs> criterion = null) {
			Context = context;
			Message = message;

			Criterion = criterion ?? new ReactionFromSourceUser(context.Member.Id);

			this._deleteEmote = deleteEmote;

			this._deleteSemaphore = new SemaphoreSlim(1);
		}

		public Task InitialiseAsync() {
			return Task.CompletedTask;
		}

		public Task HandleTimeoutAsync() {
			return this._isDeleted ? Task.CompletedTask : Message.DeleteAsync();
		}

		public async Task<bool> HandleCallbackAsync(ReactionAddedEventArgs args) {
			await this._deleteSemaphore.WaitAsync();

			if (!args.Emoji.Equals(this._deleteEmote)) {
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