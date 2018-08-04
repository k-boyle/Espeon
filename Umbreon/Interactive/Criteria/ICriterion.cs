using Discord.Commands;
using System.Threading.Tasks;

namespace Umbreon.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(ICommandContext sourceContext, T parameter);
    }
}
