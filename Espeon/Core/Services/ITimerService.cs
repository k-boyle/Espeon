using System;
using System.Threading.Tasks;
using Espeon.Core.Entities;

namespace Espeon.Core.Services
{
    public interface ITimerService
    {
        Task<int> EnqueueAsync(IRemovable removeable, Func<IRemovable, Task> removeAsync);
        Task RemoveAsync(int key);
    }
}
