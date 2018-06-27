using System.Collections.Generic;
using Discord.Addons.Interactive;
using Discord.Commands;
using Umbreon.Core.Models.Database;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
{
    public class CustomCommandsBase<T> : InteractiveBase<T> where T : class, ICommandContext
    {
        public MessageService Message { get; set; }
        public CustomCommandsService Commands { get; set; }
        public IEnumerable<CustomCommand> CurrentCmds;
        public string[] ReservedWords = { "Create", "Modify", "Delete", "Cancel", "List"};

        protected override void BeforeExecute(CommandInfo command)
        {
            CurrentCmds = Commands.GetCmds(Context);
        }
    }
}
