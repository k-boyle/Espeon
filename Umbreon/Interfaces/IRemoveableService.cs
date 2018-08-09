using System.Threading.Tasks;

namespace Umbreon.Interfaces
{
    public interface IRemoveableService
    {
        Task Remove(IRemoveable obj);
    }
}
