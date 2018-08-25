using System.Threading.Tasks;

namespace Umbreon.Interfaces
{
    public interface IRemoveableService
    {
        Task RemoveAsync(IRemoveable obj);
    }
}
