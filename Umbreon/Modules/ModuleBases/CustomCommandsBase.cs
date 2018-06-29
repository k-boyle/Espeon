using Discord.Commands;
using System.Collections.Generic;
using Umbreon.Core.Models.Database;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
{
    public class CustomCommandsBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public CustomCommandsService Commands { get; set; }
        public IEnumerable<CustomCommand> CurrentCmds;
        public string[] ReservedWords = { "Create", "Modify", "Delete", "Cancel", "List"};

        protected override void BeforeExecute(CommandInfo command)
        {
            CurrentCmds = Commands.GetCmds(Context);
        }
    }
}
