using Espeon.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    //TODO renamed this stuff
    /*
     * Summaries
     * Checks?
     */
    [Name("Management")]
    [RequireOwner]
    public class Management : EspeonBase
    {
        public CommandManagementService Manager { get; set; }
        
        [Command("Alias")]
        [Name("Command Alias")]
        public async Task CommandAliasAsync(Alias action, Command target, string value)
        {
            bool result;

            switch (action)
            {
                case Alias.Add:

                    result = await Manager.AddAliasAsync(Context, target.Module, target.Name, value);

                    if (result)
                    {
                        await SendOkAsync(0, value, target.Name);
                        return;
                    }

                    await SendNotOkAsync(1, value, target.Name);

                    break;
                case Alias.Remove:

                    result = await Manager.RemoveAliasAsync(Context, target.Module, target.Name, value);

                    if (result)
                    {
                        await SendOkAsync(2, value, target.Name);
                        return;
                    }

                    await SendNotOkAsync(3, value, target.Name);

                    break;
            }
        }

        [Command("Alias")]
        [Name("Module Alias")]
        public async Task ModuleAliasAsync(Alias action, Module target, string value)
        {
            bool result;

            switch (action)
            {
                case Alias.Add:

                    result = await Manager.AddAliasAsync(Context, target, value);

                    if (result)
                    {
                        await SendOkAsync(0, value, target.Name);
                        return;
                    }

                    await SendNotOkAsync(1, value, target.Name);

                    break;
                case Alias.Remove:

                    result = await Manager.RemoveAliasAsync(Context, target, value);

                    if (result)
                    {
                        await SendOkAsync(2, value, target.Name);
                        return;
                    }

                    await SendNotOkAsync(3, value, target.Name);

                    break;
            }
        }
    }
}
