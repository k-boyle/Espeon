using Discord;
using Discord.WebSocket;
using Espeon.Core.Commands;
using Espeon.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public abstract class PaginatorBase : IReactionCallback {
		public abstract EspeonContext Context { get; }

		public bool RunOnGatewayThread => false;

		public abstract IInteractiveService Interactive { get; }
		public abstract IMessageService MessageService { get; }
		public abstract PaginatorOptions Options { get; }

		public abstract ICriterion<SocketReaction> Criterion { get; }

		public IUserMessage Message { get; private set; }
		public IEnumerable<IEmote> Reactions => Options.Controls.Keys;

		private int _currentPage;
		private int _lastPage;
		private bool _manageMessages;

		public virtual async Task InitialiseAsync() {
			this._manageMessages = Context.Guild.CurrentUser.GetPermissions(Context.Channel).ManageMessages;

			(string content, Embed embed) = GetCurrentPage();
			Message = await MessageService.SendAsync(Context, x => {
				x.Content = content;
				x.Embed = embed;
			});
		}

		public virtual Task HandleTimeoutAsync() {
			return Message.DeleteAsync();
		}

		public virtual async Task<bool> HandleCallbackAsync(SocketReaction reaction) {
			IEmote emote = reaction.Emote;
			this._lastPage = this._currentPage;

			if (this._manageMessages) {
				SocketGuildUser user = Context.Guild.GetUser(reaction.UserId);

				if (!(user is null)) {
					await Message.RemoveReactionAsync(emote, user);
				}
			}

			switch (Options.Controls[emote]) {
				case Control.First:
					this._currentPage = 0;
					break;

				case Control.Last:
					this._currentPage = Options.Pages.Count - 1;
					break;

				case Control.Previous:
					if (this._currentPage == 0) {
						goto case Control.Last;
					}

					this._currentPage--;
					break;

				case Control.Next:
					if (this._currentPage == Options.Pages.Count - 1) {
						goto case Control.First; //_currentPage = 0 is lame
					}

					this._currentPage++;
					break;

				case Control.Delete:
					await HandleTimeoutAsync();
					return true;

				case Control.Skip:
					await MessageService.SendAsync(Context, x => x.Content = "What page would you like to skip to?");

					SocketUserMessage reply = await Interactive.NextMessageAsync(Context,
						new MultiCriteria<SocketUserMessage>(new UserCriteria(Context.User.Id),
							new ChannelCriteria(Context.Channel.Id)));

					if (int.TryParse(reply.Content, out int page)) {
						if (page >= 0 && page < Options.Pages.Count - 1) {
							this._currentPage = page;
						}
					} else {
						await MessageService.SendAsync(Context, x => x.Content = "Index was out of range");
					}

					break;

				case Control.Info:
					await MessageService.SendAsync(Context,
						x => x.Content = "I am a paginated message! You can press the reactions below to control me!");
					break;

				default:
					return false;
			}

			if (this._currentPage == this._lastPage) {
				return false;
			}

			(string content, Embed embed) = GetCurrentPage();

			await Message.ModifyAsync(x => {
				x.Content = content;
				x.Embed = embed;
			});

			return false;
		}

		private (string Content, Embed Embed) GetCurrentPage() {
			return Options.Pages[this._currentPage];
		}
	}
}