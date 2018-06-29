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
            var targetCmd = cmds.FirstOrDefault(x => string.Equals(x.Name, input, StringComparison.CurrentCultureIgnoreCase));
            targetCmd = targetCmd ?? cmds.FirstOrDefault(x => x.Name.Contains(input));
            return targetCmd is null
                ? TypeReaderResult.FromError(new NotFoundResult("Command not found", false,
                    CommandError.ObjectNotFound))
                : (await targetCmd.CheckPreconditionsAsync(context, services)).IsSuccess
                    ? TypeReaderResult.FromSuccess(targetCmd)
                    : TypeReaderResult.FromError(new FailedPreconditionResult("You do not have permission to view this command", false, CommandError.UnmetPrecondition));
        }
    }
}
