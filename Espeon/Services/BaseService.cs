using Espeon.Database;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public abstract class BaseService
    {
        public virtual Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
            => Task.CompletedTask;
    }
}
