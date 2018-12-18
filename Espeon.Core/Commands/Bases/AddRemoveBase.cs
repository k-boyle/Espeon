using System.Threading.Tasks;

namespace Espeon.Core.Commands.Bases
{
    public abstract class AddRemoveBase<TTarget, TValue> : EspeonBase
    {
        public abstract Task AddAsync(TTarget target, TValue value);
        public abstract Task RemoveAsync(TTarget target, TValue value);
    }
}
