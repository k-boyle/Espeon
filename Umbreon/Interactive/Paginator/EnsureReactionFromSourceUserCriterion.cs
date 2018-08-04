using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Umbreon.Interactive.Paginator
{
    internal class EnsureReactionFromSourceUserCriterion : Criteria.ICriterion<SocketReaction>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, SocketReaction parameter)
        {
            bool ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}
