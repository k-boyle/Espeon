using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class CommandTypeParser : TypeParser<Command>
    {
        public override ValueTask<TypeParserResult<Command>> ParseAsync(Parameter param, string value, CommandContext context, IServiceProvider provider)
        {
            var commands = provider.GetService<CommandService>();
            var command = commands.GetAllCommands().SingleOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            return new ValueTask<TypeParserResult<Command>>(command is null
                ? new TypeParserResult<Command>("Multiple or no matching commands")
                : new TypeParserResult<Command>(command));
        }
    }
}
