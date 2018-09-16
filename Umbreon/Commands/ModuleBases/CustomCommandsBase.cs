using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.ModuleBases
{
    public class CustomCommandsBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public CustomCommandsService Commands { get; set; }
        public Task<IEnumerable<CustomCommand>> CurrentCmds => Commands.GetCmdsAsync(Context);
        public string[] ReservedWords = { "Create", "Modify", "Delete", "Cancel", "List", "c" };
    }
}
