using Discord;
using Discord.WebSocket;
using Espeon.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public abstract class PaginatorBase : IReactionCallback
    {
        public abstract EspeonContext Context { get; }

        public bool RunOnGatewayThread => true;

        public abstract InteractiveService Interactive { get; }
        public abstract MessageService MessageService { get; }
        public abstract PaginatorOptions Options { get; }

        public abstract ICriterion<SocketReaction> Criterion { get; }

        public IUserMessage Message { get; private set; }
        public IEnumerable<IEmote> Reactions => Options.Controls.Keys;

        private int _currentPage;
        private int _lastPage;
        private bool _manageMessages;

        public virtual async Task InitialiseAsync()
        {
            _manageMessages = Context.Guild.CurrentUser.GetPermissions(Context.Channel).ManageMessages;

            var (content, embed) = GetCurrentPage();
            Message = await MessageService.SendAsync(Context, x =>
            {
                x.Content = content;
                x.Embed = embed;
            });
        }

        public virtual Task HandleTimeoutAsync()
        {
            return Message.DeleteAsync();
        }

        public virtual async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;
            _lastPage = _currentPage;

            if (_manageMessages)
            {
                var user = Context.Guild.GetUser(reaction.UserId);

                if (!(user is null))
                    await Message.RemoveReactionAsync(emote, user);
            }

            switch (Options.Controls[emote])
            {
                case Control.First:
                    _currentPage = 0;
                    break;

                case Control.Last:
                    _currentPage = Options.Pages.Count - 1;
                    break;

                case Control.Previous:
                    if (_currentPage == 0)
                        goto case Control.Last;

                    _currentPage--;
                    break;

                case Control.Next:
                    if (_currentPage == Options.Pages.Count - 1)
                        goto case Control.First; //_currentPage = 0 is lame

                    _currentPage++;
                    break;

                case Control.Delete:
                    await HandleTimeoutAsync();
                    return true;

                case Control.Skip:
                    await MessageService.SendAsync(Context, 
                        x => x.Content = "What page would you like to skip to?");

                    var reply = await Interactive.NextMessageAsync(Context,
                        new MultiCriteria<SocketUserMessage>(new UserCriteria(Context.User.Id),
                            new ChannelCriteria(Context.Channel.Id)));

                    if (int.TryParse(reply.Content, out var page))
                    {
                        if (page > 0 && page < Options.Pages.Count - 1)
                        {
                            _currentPage = page;
                        }
                    }
                    break;

                case Control.Info:
                    await MessageService.SendAsync(Context,
                        x => x.Content = "I am a paginated message! You can press the reactions below to control me!");
                    break;

                default:
                    return false;
            }

            if (_currentPage == _lastPage)
                return false;

            var (content, embed) = GetCurrentPage();

            await Message.ModifyAsync(x =>
            {
                x.Content = content;
                x.Embed = embed;
            });

            return false;
        }

        private (string Content, Embed Embed) GetCurrentPage()
            => Options.Pages[_currentPage];
    }
}
