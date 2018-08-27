using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Commands.Preconditions;

namespace Umbreon.Commands.TypeReaders
{
    public class CommandInfoTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var commands = services.GetService<CommandService>();
            var cmds = commands.Commands;
            var targetCmds = cmds.Where(x => string.Equals(x.Name, input, StringComparison.CurrentCultureIgnoreCase));
            targetCmds = targetCmds.Count() > 0 ? targetCmds : cmds.Where(x => x.Name.Contains(input));
            return targetCmds.Count() == 0 ? Task.FromResult(TypeReaderResult.FromError(new FailedResult("No commands found", false, CommandError.UnknownCommand))) : Task.FromResult(TypeReaderResult.FromSuccess(targetCmds.Where(x => x.CheckPreconditionsAsync(context, services).Result.IsSuccess)));
        }
    }
}
