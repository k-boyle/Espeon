using System;
using System.Threading.Tasks;
using Espeon.Core.Entities;

namespace Espeon.Core.Services
{
    public interface ITimerService
    {
        Task<string> EnqueueAsync(IRemovable removable, Func<string, IRemovable, Task> removeAsync);
        Task RemoveAsync(string key);
    }
}
