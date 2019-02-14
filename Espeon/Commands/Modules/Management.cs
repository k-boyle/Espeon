using Espeon.Commands.Checks;
using Espeon.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    //TODO renamed this stuff
    /*
     * Summaries
     * Checks?
     */
    [Name("Module Management")]
    [Group("Module")]
    [RequireOwner]
    public class ModuleManagement : EspeonBase
    {
        public ModuleManager Manager { get; set; }

        [Command("Add")]
        [Name("Module Add Alias")]
        public async Task AddAsync(Module target, string value)
        {
            var result = await Manager.AddAliasAsync(Context, target, value);

            if (result)
            {
                await SendOkAsync(0, value, target.Name);
                return;
            }

            await SendNotOkAsync(1, value, target.Name);
        }

        [Command("Remove")]
        [Name("Module Remove Alias")]
        public async Task RemoveAsync(Module target, string value)
        {
            var result = await Manager.RemoveAliasAsync(Context, target, value);

            if (result)
            {
                await SendOkAsync(0, value, target.Name);
                return;
            }

            await SendNotOkAsync(1, value, target.Name);
        }
    }

    [Name("Command Management")]
    [Group("Command")]
    [RequireOwner]
    public class CommandManagement : EspeonBase
    {
        public ModuleManager Manager { get; set; }

        [Command("Add")]
        [Name("Command Add Alias")]
        public async Task AddAsync(Command target, string value)
        {
            var result = await Manager.AddAliasAsync(Context, target.Module, target.Name, value);

            if (result)
            {
                await SendOkAsync(0, value, target.Name);
                return;
            }

            await SendNotOkAsync(1, value, target.Name);
        }

        [Command("Remove")]
        [Name("Command Remove Alias")]
        public async Task RemoveAsync(Command target, string value)
        {
            var result = await Manager.RemoveAliasAsync(Context, target.Module, target.Name, value);

            if (result)
            {
                await SendOkAsync(0, value, target.Name);
                return;
            }

            await SendNotOkAsync(1, value, target.Name);
        }
    }
}
