using Espeon.Core.Commands.Checks;
using Espeon.Core.Services;
using Qmmands;
using System.Threading.Tasks;
using Base = Espeon.Core.Commands.Modules;

namespace Espeon.Commands.Modules
{
    [Name("Module Management")]
    [Group("Module")]
    [RequireOwner]
    public class ModuleManagement : Base.ModuleManagement
    {
        public override IModuleManager Manager { get; set; }

        [Command("Add")]
        [Name("Module Add Alias")]
        public override async Task AddAsync(Module target, string value)
        {
            var result = await Manager.AddAliasAsync(target, value);
            var response = ResponseBuilder.Message(Context,
                result ? $"{value} has been added to {target.Name}!" : $"{value} already exists for {target.Name}!",
                result);

            await SendMessageAsync(response);
        }

        [Command("Remove")]
        [Name("Module Remove Alias")]
        public override async Task RemoveAsync(Module target, string value)
        {
            var result = await Manager.RemoveAliasAsync(target, value);
            var response = ResponseBuilder.Message(Context,
                result ? $"{value} has been removed from {target.Name}!" : $"{value} doesn't exist for {target.Name}!",
                result);

            await SendMessageAsync(response);
        }
    }

    [Name("Command Management")]
    [Group("Command")]
    [RequireOwner]
    public class CommandManagement : Base.CommandManagement
    {
        public override IModuleManager Manager { get; set; }

        [Command("Add")]
        [Name("Command Add Alias")]
        public override async Task AddAsync(Command target, string value)
        {
            var result = await Manager.AddAliasAsync(target.Module, target.Name, value);
            var response = ResponseBuilder.Message(Context,
                result ? $"{value} has been added to {target.Name}!" : $"{value} already exists for {target.Name}!",
                result);

            await SendMessageAsync(response);
        }

        [Command("Remove")]
        [Name("Command Add Alias")]
        public override async Task RemoveAsync(Command target, string value)
        {
            var result = await Manager.RemoveAliasAsync(target.Module, target.Name, value);
            var response = ResponseBuilder.Message(Context,
                result ? $"{value} has been removed from {target.Name}!" : $"{value} doesn't exist for {target.Name}!",
                result);

            await SendMessageAsync(response);
        }
    }
}
