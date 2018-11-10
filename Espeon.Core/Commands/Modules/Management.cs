using Espeon.Core.Commands.Bases;
using Espeon.Core.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Modules
{
    [Name("Module Management")]
    [Group("Module")]
    public class ModuleManagement : AddRemoveBase<Module, string>
    {
        public IModuleManager Manager { get; set; }

        [Command("Add")]
        public override async Task<EspeonResult> AddAsync(Module target, string value)
        {
            var result = await Manager.AddAliasAsync(target, value);
            return new EspeonResult(result, result ? "Alias has been added!" : "Alias already exists for this module!");
        }

        [Command("Remove")]
        public override async Task<EspeonResult> RemoveAsync(Module target, string value)
        {
            var result = await Manager.RemoveAliasAsync(target, value);
            return new EspeonResult(result, result ? "Alias has been removed!" : "Alias doesn't exist for this module!");
        }
    }
}

