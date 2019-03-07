using Espeon.Commands;
using System.Threading.Tasks;

namespace Espeon.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(EspeonContext context, T entity);
    }
}
