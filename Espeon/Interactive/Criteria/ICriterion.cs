using Discord.Commands;
using System.Threading.Tasks;

namespace Espeon.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(ICommandContext sourceContext, T parameter);
    }
}
