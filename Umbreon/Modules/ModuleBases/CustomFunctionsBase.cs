using Discord.Commands;
using System.Collections.Generic;
using Umbreon.Core.Models.Database;
using Umbreon.Core.Models.Database.Guilds;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
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
