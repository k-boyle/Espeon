using Qmmands;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface IResponseService
    {
        Task<string> GetResponseAsync(Module module, Command command, string pack, params object[] @params);
        Task OnCommandsRegisteredAsync(IEnumerable<Module> modules);
        Task<ImmutableArray<string>> GetResponsesPacksAsync();
        Task<string> GetUsersPackAsync(ulong id);
    }
}