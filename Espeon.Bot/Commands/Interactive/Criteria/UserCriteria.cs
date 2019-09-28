using Discord;
using Espeon.Commands;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class UserCriteria : ICriterion<IUser>, ICriterion<IMessage>
    {
        private readonly ulong _userId;

        public UserCriteria(ulong userId)
        {
            _userId = userId;
        }

        public Task<bool> JudgeAsync(EspeonContext context, IUser entity)
        {
            return Task.FromResult(entity.Id == _userId);
        }

        public Task<bool> JudgeAsync(EspeonContext context, IMessage entity)
        {
            return Task.FromResult(_userId == entity.Author.Id);
        }
    }
}
