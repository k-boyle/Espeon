using System.Threading.Tasks;

namespace Espeon.Core.Commands.Bases
{
    public abstract class AddRemoveBase<TTarget, TValue> : EspeonBase
    {
        public abstract Task<EspeonResult> AddAsync(TTarget target, TValue value);
        public abstract Task<EspeonResult> RemoveAsync(TTarget target, TValue value);
    }
}
