using System;
using System.Threading.Tasks;
using Espeon.Core.Entities;

namespace Espeon.Core.Services
{
    public interface ITimerService
    {
        Task EnqueueAsync(IRemovable removeable, Func<IRemovable, Task> removeAsync);
        Task RemoveAsync(IRemovable removeable);
    }
}
