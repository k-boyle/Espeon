using System.Threading.Tasks;

namespace Espeon.Commands
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(EspeonContext context, T entity);
    }
}
