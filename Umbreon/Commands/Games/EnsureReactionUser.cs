using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Umbreon.Interactive.Criteria;

namespace Umbreon.Commands.Games
{
    public class EnsureReactionUser : ICriterion<SocketReaction>
    {
        private readonly ulong _id;

        public EnsureReactionUser(ulong id)
            => _id = id;

        public Task<bool> JudgeAsync(ICommandContext sourceContext, SocketReaction parameter)
            => Task.FromResult(_id == parameter.UserId);
    }
}
