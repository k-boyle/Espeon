using System.Threading.Tasks;
using Discord.Commands;

namespace Umbreon.Interactive.Criteria
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, T parameter)
            => Task.FromResult(true);
    }
}
