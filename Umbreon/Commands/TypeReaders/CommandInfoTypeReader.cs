using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Commands.Preconditions;

namespace Umbreon.Commands.TypeReaders
{
    public class CommandInfoTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var commands = services.GetService<CommandService>();
            var cmds = commands.Commands;
            var commandInfos = cmds as CommandInfo[] ?? cmds.ToArray();
            var targetCmds = commandInfos.Where(x => string.Equals(x.Name, input, StringComparison.CurrentCultureIgnoreCase)).ToImmutableArray();
            targetCmds = (ImmutableArray<CommandInfo>) (targetCmds.Length > 0 ? targetCmds : commandInfos.Where(x => x.Name.Contains(input)));
            return targetCmds.Length == 0 ? Task.FromResult(TypeReaderResult.FromError(new FailedResult("No commands found", false, CommandError.UnknownCommand))) : Task.FromResult(TypeReaderResult.FromSuccess(targetCmds.Where(x => x.CheckPreconditionsAsync(context, services).Result.IsSuccess)));
        }
    }
}
