using Discord.WebSocket;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class ReactionFromSourceUser : ICriterion<SocketReaction>
    {
        private readonly ulong _userId;

        public ReactionFromSourceUser(ulong userId)
        {
            _userId = userId;
        }

        public Task<bool> JudgeAsync(EspeonContext context, SocketReaction reaction)
            => Task.FromResult(reaction.UserId == _userId);
    }
}
