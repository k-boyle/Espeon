using System.Threading.Tasks;
using Discord.Commands;

namespace Umbreon.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(ICommandContext sourceContext, T parameter);
    }
}
