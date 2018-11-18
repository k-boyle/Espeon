using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface IModuleManager
    {
        //Task OnBuildingAsync(ModuleBuilder builder);
        Task<bool> AddAliasAsync(Module module, string alias);
        Task<bool> AddAliasAsync(Module module, string command, string alias);
        Task<bool> RemoveAliasAsync(Module module, string alias);
        Task<bool> RemoveAliasAsync(Module module, string command, string alias);
    }
}
