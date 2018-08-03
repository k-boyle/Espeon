/*
 * All code under Umbreon.Interactive is using
 * Discord.Addons.Interactive
 * https://github.com/foxbot/Discord.Addons.Interactive
 */

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbreon.Interactive.Paginator;
using Umbreon.Interactive.Results;

namespace Umbreon.Interactive
{
    public class InteractiveBase : InteractiveBase<ICommandContext>
    {
    }

    public class InteractiveBase<T> : ModuleBase<T>
        where T : class, ICommandContext
    {
        public InteractiveService Interactive { get; set; }

        public Task<SocketMessage> NextMessageAsync(Umbreon.Interactive.Criteria.ICriterion<SocketMessage> criterion, TimeSpan? timeout = null)
            => Interactive.NextMessageAsync(Context, criterion, timeout);
        public Task<SocketMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true, TimeSpan? timeout = null) 
            => Interactive.NextMessageAsync(Context, fromSourceUser, inSourceChannel, timeout);

        public Task<IUserMessage> ReplyAndDeleteAsync(string content, bool isTts = false, Embed embed = null, TimeSpan? timeout = null, RequestOptions options = null)
            => Interactive.ReplyAndDeleteAsync(Context, content, isTts, embed, timeout, options);

        public Task<IUserMessage> PagedReplyAsync(IEnumerable<object> pages, bool fromSourceUser = true)
        {
            var pager = new PaginatedMessage
            {
                Pages = pages
            };
            return PagedReplyAsync(pager, fromSourceUser);
        }
        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, bool fromSourceUser = true)
        {
            var criterion = new Umbreon.Interactive.Criteria.Criteria<SocketReaction>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());
            return PagedReplyAsync(pager, criterion);
        }
        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, Umbreon.Interactive.Criteria.ICriterion<SocketReaction> criterion)
            => Interactive.SendPaginatedMessageAsync(Context, pager, criterion);
        
        public RuntimeResult Ok(string reason = null) => new OkResult(reason);
    }
}
