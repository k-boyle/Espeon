using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
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

            if (command is null)
            {
                var resp = new Dictionary<ResponsePack, string>
                {
                    [ResponsePack.Default] = "Multiple or no matching commands",
                    [ResponsePack.owo] = "multwiple or no mwaching cwommands"
                };

                return new TypeParserResult<Command>(resp[context.Invoker.ResponsePack]);
            }

            return new TypeParserResult<Command>(command);
        }
    }
}
