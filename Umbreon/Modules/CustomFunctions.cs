using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core.Models.Database;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;

namespace Umbreon.Modules
{
    [Group("func")]
    [Name("Custom Functions")]
    [Summary("Create custom functions for the bot")]
    [RequireOwner]
    public class CustomFunctions : CustomFunctionsBase<GuildCommandContext>
    {
        [Command("create")]
        [Summary("Create a function")]
        [Usage("func create")]
        [Name("Create Function")]
        public async Task CreateFunction(
            [Name("Function")]
            [Summary("The function you want to create")]
            [Remainder] CustomFunction func)
        {
            if (Funcs.IsReserved(func.FunctionName))
            {
                await SendMessageAsync("This function already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, func.FunctionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This is a reserved word");
                return;
            }

            await Funcs.NewFunc(Context, func);
            await SendMessageAsync("Function has been created");
        }

        [Command("Delete")]
        [Summary("Delete a function")]
        [Usage("func delete SomeName")]
        [Name("Delete Function")]
        public async Task DeleteFunction(
            [Name("Function Name")]
            [Summary("The name of the function")]
            [Remainder] string name)
        {
            var found = CurrentFuncs.FirstOrDefault(x => string.Equals(x.FunctionName, name, StringComparison.CurrentCultureIgnoreCase));
            if (found is null)
            {
                await SendMessageAsync("Function not found");
                return;
            }
            await Funcs.RemoveFunc(Context, found);
            await SendMessageAsync("Function has been removed");
        }

        [Command("Modify")]
        [Summary("Modify a function")]
        [Usage("func modify SomeName")]
        [Name("Modify Function")]
        public async Task ModifyFunction(
            [Name("Function Name")]
            [Summary("The function you want to modify")]string name,
            [Name("New Function")]
            [Summary("The new callback for the function")]
            [Remainder] CustomFunction func)
        {
            var found = CurrentFuncs.FirstOrDefault(x => string.Equals(x.FunctionName, name, StringComparison.CurrentCultureIgnoreCase));
            if (found is null)
            {
                await SendMessageAsync("Function not found");
                return;
            }

            if (Funcs.IsReserved(func.FunctionName) && func.FunctionName != found.FunctionName)
            {
                await SendMessageAsync("This function already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, func.FunctionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This is a reserved word");
                return;
            }

            await Funcs.UpdateFunction(Context, found, func);
            await SendMessageAsync("Function has been updated");
        }
    }
}
