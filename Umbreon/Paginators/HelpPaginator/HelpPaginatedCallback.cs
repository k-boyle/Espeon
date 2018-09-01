using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Umbreon.Extensions;
using Umbreon.Helpers;
using Umbreon.Interactive;
using Umbreon.Interactive.Callbacks;
using Umbreon.Interactive.Criteria;
using Umbreon.Interactive.Paginator;
using Colour = Discord.Color;

namespace Umbreon.Paginators.HelpPaginator
{
    public class HelpPaginatedCallback : IReactionCallback, ICallback
    {
        private readonly HelpPaginatedMessage _pager;
        private readonly InteractiveService _interactive;
        private readonly int _pages;

        private int _page = 1;
        private PaginatedAppearanceOptions Options => _pager.Options;

        public IUserMessage Message { get; private set; }
        public ICommandContext Context { get; }
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);

        public HelpPaginatedCallback(InteractiveService interactive,
            ICommandContext sourceContext, HelpPaginatedMessage pager)
        {
            _interactive = interactive;
            Context = sourceContext;
            _pager = pager;
            _pages = _pager.Pages.Count();
        }

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(string.Empty, embed: embed).ConfigureAwait(false);
            Message = message;
            _interactive.AddReactionCallback(message, this);
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

                var manageMessages = Context.Channel is IGuildChannel guildChannel
                    ? (Context.User as IGuildUser).GetPermissions(guildChannel).ManageMessages
                    : false;

                if (Options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages)
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
            if (Timeout.HasValue && Timeout.Value != null)
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    _interactive.RemoveReactionCallback(message);
                    _ = Message.DeleteAsync();
                });
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(Options.First))
            {
                _page = 1;
            }
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
            {
                _page = _pages;
            }
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
                    var response = await _interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > _pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await _interactive.ReplyAndDeleteAsync(Context, Options.Stop.Name);
                        return;
                    }

                    _page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(Options.Info))
            {
                await _interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout);
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
                .WithColor(Colour.Gold)
                .WithDescription($"Type {_pager.Prefix}help CommandName to view more help for that command!\n" +
                                 $"e.g. {_pager.Prefix}help create tag")
                .AddField(GetPage(_pager.Pages, _page - 1).Title)
                .AddField(EmbedHelper.EmptyField())
                .AddFields(GetPage(_pager.Pages, _page - 1).Fields)
                .WithFooter($"Page: {_page}/{_pages}")
                .Build();
        }

        private Page GetPage(IEnumerable<Page> pages, int index)
        {
            return pages.ElementAt(index);
        }

        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}