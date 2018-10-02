using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Espeon.Interactive.Criteria;

namespace Espeon.Interactive.Paginator
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, SocketReaction parameter)
        {
            bool ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}
