using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class CommandTypeParser : TypeParser<Command>
    {
        public override ValueTask<TypeParserResult<Command>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = (EspeonContext)ctx;

            var commands = provider.GetService<CommandService>();
            var command = commands.GetAllCommands().SingleOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            if (!(command is null))
                return new TypeParserResult<Command>(command);

            var response = provider.GetService<ResponseService>();
            var user = context.Invoker;

            return new TypeParserResult<Command>(
                response.GetResponse(this, user.ResponsePack, 0));

        }
    }
}
