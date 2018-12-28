using Espeon.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    [Name("Module Management")]
    [Group("Module")]
    [Checks.RequireOwner]
    public class ModuleManagement : EspeonBase
    {
        public ModuleManager Manager { get; set; }

        [Command("Add")]
        [Name("Module Add Alias")]
        public async Task AddAsync(Module target, string value)
        {
            var result = await Manager.AddAliasAsync(Context, target, value);
            var response = ResponseBuilder.Message(Context,
                result ? $"{value} has been added to {target.Name}!" : $"{value} already exists for {target.Name}!",
                result);

            await SendMessageAsync(response);
        }

        [Command("Remove")]
        [Name("Module Remove Alias")]
        public async Task RemoveAsync(Module target, string value)
        {
            var result = await Manager.RemoveAliasAsync(Context, target, value);
            var response = ResponseBuilder.Message(Context,
                result ? $"{value} has been removed from {target.Name}!" : $"{value} doesn't exist for {target.Name}!",
                result);

            await SendMessageAsync(response);
        }
    }

    [Name("Command Management")]
    [Group("Command")]
    [Checks.RequireOwner]
    public class CommandManagement : EspeonBase
    {
        public ModuleManager Manager { get; set; }

        [Command("Add")]
        [Name("Command Add Alias")]
        public async Task AddAsync(Command target, string value)
        {
            var result = await Manager.AddAliasAsync(Context, target.Module, target.Name, value);
            var response = ResponseBuilder.Message(Context,
                result ? $"{value} has been added to {target.Name}!" : $"{value} already exists for {target.Name}!",
                result);

            await SendMessageAsync(response);
        }

        [Command("Remove")]
        [Name("Command Remove Alias")]
        public async Task RemoveAsync(Command target, string value)
        {
            var result = await Manager.RemoveAliasAsync(Context, target.Module, target.Name, value);
            var response = ResponseBuilder.Message(Context,
                result ? $"{value} has been removed from {target.Name}!" : $"{value} doesn't exist for {target.Name}!",
                result);

            await SendMessageAsync(response);
        }
    }
}
