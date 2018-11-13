using Espeon.Core.Commands.Bases;
using Espeon.Core.Commands.Checks;
using Espeon.Core.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Modules
{
    [Name("Module Management")]
    [Group("Module")]
    [RequireOwner]
    public class ModuleManagement : AddRemoveBase<Module, string>
    {
        public IModuleManager Manager { get; set; }

        [Command("Add")]
        [Name("Module Add Alias")]
        public override async Task<EspeonResult> AddAsync(Module target, string value)
        {
            var result = await Manager.AddAliasAsync(target, value);
            return new EspeonResult(result, result ? $"{value} has been added to {target.Name}!" : $"{value} already exists for {target.Name}!");
        }

        [Command("Remove")]
        [Name("Module Remove Alias")]
        public override async Task<EspeonResult> RemoveAsync(Module target, string value)
        {
            var result = await Manager.RemoveAliasAsync(target, value);
            return new EspeonResult(result, result ? $"{value} has been removed from {target.Name}!" : $"{value} doesn't exist for {target.Name}!");
        }
    }

    [Name("Command Management")]
    [Group("Command")]
    [RequireOwner]
    public class CommandManagement : AddRemoveBase<Command, string>
    {
        public IModuleManager Manager { get; set; }

        [Command("Add")]
        [Name("Command Add Alias")]
        public override async Task<EspeonResult> AddAsync(Command target, string value)
        {
            var result = await Manager.AddAliasAsync(target.Module, target.Name, value);
            return new EspeonResult(result, result ? $"{value} has been added to {target.Name}!" : $"{value} already exists for {target.Name}!");
        }

        [Command("Remove")]
        [Name("Command Add Alias")]
        public override async Task<EspeonResult> RemoveAsync(Command target, string value)
        {
            var result = await Manager.RemoveAliasAsync(target.Module, target.Name, value);
            return new EspeonResult(result, result ? $"{value} has been removed from {target.Name}!" : $"{value} doesn't exist for {target.Name}!");
        }
    }
}

