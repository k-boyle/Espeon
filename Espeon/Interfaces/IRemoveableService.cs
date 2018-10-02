using System.Threading.Tasks;

namespace Espeon.Interfaces
{
    public interface IRemoveableService
    {
        Task RemoveAsync(IRemoveable obj);
    }
}
