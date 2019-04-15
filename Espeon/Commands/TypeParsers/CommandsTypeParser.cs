using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class CommandsTypeParser : TypeParser<IReadOnlyCollection<Command>>
    {
        public override async ValueTask<TypeParserResult<IReadOnlyCollection<Command>>> ParseAsync(Parameter param, string value, CommandContext context, IServiceProvider provider)
        {
            var service = provider.GetService<CommandService>();
            var commands = service.GetAllCommands();

            var found = commands.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) 
                || x.FullAliases.Any(y => y.Contains(value, StringComparison.InvariantCultureIgnoreCase))).ToArray();

            var canExecute = new List<Command>();

            foreach(var command in found)
            {
                var result = await command.RunChecksAsync(context, provider);

                if(result.IsSuccessful)
                {
                    canExecute.Add(command);
                }
            }

            return canExecute.Count == 0 
                ? new TypeParserResult<IReadOnlyCollection<Command>>($"Failed to find any commands matching {value}") 
                : new TypeParserResult<IReadOnlyCollection<Command>>(found);
        }
    }
}
