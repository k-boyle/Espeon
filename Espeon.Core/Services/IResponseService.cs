using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;

namespace Espeon.Core.Services
{
    public interface IResponseService
    {
        Task<string> GetResponseAsync(Module module, Command command, string pack, params string[] @params);
        Task OnCommandsRegisteredAsync(IEnumerable<Module> modules);
    }
}