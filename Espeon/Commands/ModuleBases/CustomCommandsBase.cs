using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using Espeon.Core.Entities.Guild;
using Espeon.Services;

namespace Espeon.Commands.ModuleBases
{
    public class CustomCommandsBase<T> : EspeonBase<T> where T : class, ICommandContext
    {
        public CustomCommandsService Commands { get; set; }
        public Task<IEnumerable<CustomCommand>> CurrentCmdsAsync => Commands.GetCmdsAsync(Context);
        public string[] ReservedWords = { "Create", "Modify", "Delete", "Cancel", "List", "c" };
    }
}
