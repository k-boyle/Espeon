using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Results;

namespace Umbreon.TypeReaders
{
    public class CommandInfoTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var commands = services.GetService<CommandService>();
            var cmds = commands.Commands;
            var targetCmds = cmds.Where(x => string.Equals(x.Name, input, StringComparison.CurrentCultureIgnoreCase));
            targetCmds = targetCmds.Any() ? targetCmds : cmds.Where(x => x.Name.Contains(input));
            return !targetCmds.Any() ? TypeReaderResult.FromError(new NotFoundResult("No commands found", false, CommandError.UnknownCommand)) : TypeReaderResult.FromSuccess(targetCmds.Where(x => x.CheckPreconditionsAsync(context, services).Result.IsSuccess));
        }
    }
}
