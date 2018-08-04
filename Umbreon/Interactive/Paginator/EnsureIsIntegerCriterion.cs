using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Umbreon.Interactive.Paginator
{
    internal class EnsureIsIntegerCriterion : Criteria.ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, SocketMessage parameter)
        {
            bool ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}
