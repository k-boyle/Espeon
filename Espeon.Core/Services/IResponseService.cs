using System.Threading.Tasks;
using Qmmands;

namespace Espeon.Core.Services
{
    public interface IResponseService
    {
        Task<string> GetResponseAsync(Module module, Command command);
    }
}