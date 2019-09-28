using Espeon.Commands;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public interface ICommandManagementService
    {
        Task<bool> AddAliasAsync(EspeonContext context, Module module, string alias);
        Task<bool> AddAliasAsync(EspeonContext context, Module module, string command, string alias);
        Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string alias);
        Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string command, string alias);
    }
}
