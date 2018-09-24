using Discord.Commands;
using System.Threading.Tasks;

namespace Espeon.Interactive.Criteria
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, T parameter)
            => Task.FromResult(true);
    }
}
