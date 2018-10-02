using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Espeon.Interactive.Criteria
{
    public class EnsureSourceUserCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.User.Id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}
