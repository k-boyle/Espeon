using System.Collections.Generic;
using Discord.Commands;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.ModuleBases
{
    public class CustomFunctionsBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public CustomFunctionService Funcs { get; set; }
        public IEnumerable<CustomFunction> CurrentFuncs;
        public string[] ReservedWords = { "Create", "Modify", "Delete", "c" };

        protected override void BeforeExecute(CommandInfo command)
        {
            CurrentFuncs = Funcs.GetFuncs(Context);
        }
    }
}
