using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Commands.Preconditions;

namespace Umbreon.Commands.TypeReaders
{
    public class CommandInfoTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var service = services.GetService<CommandService>();
            var commands = service.Commands;
            var matching = commands.Where(x =>
                string.Equals(input, x.Name, StringComparison.CurrentCultureIgnoreCase));

            var canExecute = new List<CommandInfo>();
            foreach (var command in matching)
                if ((await command.CheckPreconditionsAsync(context, services)).IsSuccess && command.Name != "help")
                    canExecute.Add(command);

            return canExecute.Count == 0
                ? TypeReaderResult.FromError(new FailedResult("No commands found", CommandError.Unsuccessful))
                : TypeReaderResult.FromSuccess(canExecute);
        }
    }
}
