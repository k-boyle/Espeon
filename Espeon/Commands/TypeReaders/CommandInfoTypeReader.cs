using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Espeon.Attributes;

namespace Espeon.Commands.TypeReaders
{
    [TypeReader(typeof(IEnumerable<CommandInfo>))]
    public class CommandInfoTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var service = services.GetService<CommandService>();
            var commands = service.Commands;
            var matching = commands.Where(x =>
                string.Equals(input, x.Name, StringComparison.CurrentCultureIgnoreCase) || 
                x.Name.IndexOf(input, StringComparison.CurrentCultureIgnoreCase) >= 0 || 
                x.Aliases.Any(y => y.IndexOf(input, StringComparison.CurrentCultureIgnoreCase) >= 0));

            var canExecute = new List<CommandInfo>();
            foreach (var cmd in matching)
                if ((await cmd.CheckPreconditionsAsync(context, services)).IsSuccess && cmd.Name != "help")
                    canExecute.Add(cmd);

            return canExecute.Count == 0
                ? TypeReaderResult.FromError(command, CommandError.Unsuccessful, "Failed to find any commands")
                : TypeReaderResult.FromSuccess(command, canExecute);
        }
    }
}
