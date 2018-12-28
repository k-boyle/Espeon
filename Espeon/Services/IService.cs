using Espeon.Database;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public interface IService
    {
        Task InitialiseAsync(DatabaseContext context, IServiceProvider services);
    }
}
