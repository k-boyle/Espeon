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
            return new EspeonResult(result, result ? $"{value} has been added to {target.Name}!" : $"{value} already exists for {target.Name}!");
        }

        [Command("Remove")]
        public override async Task<EspeonResult> RemoveAsync(Module target, string value)
        {
            var result = await Manager.RemoveAliasAsync(target, value);
            return new EspeonResult(result, result ? $"{value} has been removed from {target.Name}!" : $"{value} doesn't exist for {target.Name}!");
        }
    }

    [Name("Command Management")]
    [Group("Command")]
    public class CommandManagement : AddRemoveBase<Command, string>
    {
        [Command("Add")]
        public override async Task<EspeonResult> AddAsync(Command target, string value)
        {
            var result = await Manager.AddAliasAsync(target.Module, target, value);
            return new EspeonResult(result, result ? $"{value} has been added to {target.Name}!" : $"{value} already exists for {target.Name}!");
        }

        [Command("Remove")]
        public override async Task<EspeonResult> RemoveAsync(Command target, string value)
        {
            var result = await Manager.RemoveAliasAsync(target.Module, target, value);
            return new EspeonResult(result, result ? $"{value} has been removed from {target.Name}!" : $"{value} doesn't exist for {target.Name}!");
        }
    }
}

