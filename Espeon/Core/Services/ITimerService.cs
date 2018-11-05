using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface ITimerService
    {
        Task EnqueueAsync();
        Task RemoveAsync();
    }
}
