using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Criteria;
using Umbreon.Interactive.Interfaces;

namespace Umbreon.Interactive.Paginator
{
    public class PaginatedMessageCallback : IReactionCallback, ICallback
    {
        public ICommandContext Context { get; }
        public InteractiveService Interactive { get; private set; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => _criterion;
        public TimeSpan? Timeout => Options.Timeout;

        private readonly ICriterion<SocketReaction> _criterion;
        private readonly PaginatedMessage _pager;

        private PaginatedAppearanceOptions Options => _pager.Options;
        private readonly int _pages;
        private int _page = 1;
        

        public PaginatedMessageCallback(InteractiveService interactive, 
            ICommandContext sourceContext,
            PaginatedMessage pager, ICriterion<SocketReaction> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            _criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            _pager = pager;
            _pages = _pager.Pages.Count();
        }

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(_pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                await message.AddReactionAsync(Options.First, new RequestOptions
                {
                    BypassBuckets = true
                });
                await message.AddReactionAsync(Options.Back, new RequestOptions
                {
                    BypassBuckets = true
                });
                await message.AddReactionAsync(Options.Next, new RequestOptions
                {
                    BypassBuckets = true
                });
                await message.AddReactionAsync(Options.Last, new RequestOptions
                {
                    BypassBuckets = true
                });

                var manageMessages = (Context.Channel is IGuildChannel guildChannel)
                    ? (Context.User as IGuildUser).GetPermissions(guildChannel).ManageMessages
                    : false;

                if (Options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || (Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                    await message.AddReactionAsync(Options.Jump, new RequestOptions
                    {
                        BypassBuckets = true
                    });

                await message.AddReactionAsync(Options.Stop, new RequestOptions
                {
                    BypassBuckets = true
                });

                if (Options.DisplayInformationIcon)
                    await message.AddReactionAsync(Options.Info, new RequestOptions
                    {
                        BypassBuckets = true
                    });
            });
            // TODO: (Next major version) timeouts need to be handled at the service-level!
            if (Timeout.HasValue && Timeout.Value != null)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(message);
                    _ = Message.DeleteAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(Options.First))
                _page = 1;
            else if (emote.Equals(Options.Next))
            {
                if (_page >= _pages)
                    return false;
                ++_page;
            }
            else if (emote.Equals(Options.Back))
            {
                if (_page <= 1)
                    return false;
                --_page;
            }
            else if (emote.Equals(Options.Last))
                _page = _pages;
            else if (emote.Equals(Options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(Options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > _pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await Interactive.ReplyAndDeleteAsync(Context, Options.Stop.Name);
                        return;
                    }
                    _page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(Options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout);
                return false;
            }
            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }
        
        protected Embed BuildEmbed()
        {
            return new EmbedBuilder()
                .WithAuthor(_pager.Author)
                .WithColor(_pager.Color)
                .WithDescription(_pager.Pages.ElementAt(_page-1).ToString())
                .WithFooter(f => f.Text = string.Format(Options.FooterFormat, _page, _pages))
                .WithTitle(_pager.Title)
                .Build();
        }
        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}
