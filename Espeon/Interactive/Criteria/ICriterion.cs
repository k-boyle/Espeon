using System.Threading.Tasks;
using Espeon.Commands;

namespace Espeon.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeCriterionAsync(EspeonContext context, T entity);
    }
}
