using Espeon.Database.Entities;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public class CustomCommandTypeParser : TypeParser<CustomCommand>
    {
        public override Task<TypeParserResult<CustomCommand>> ParseAsync(string value, ICommandContext ctx,
            IServiceProvider provider)
        {
            if(!(ctx is EspeonContext context))
                throw new ExpectedContextException("EspeonContext");

            var service = provider.GetService<CustomCommandsService>();
            var commands = service.GetCommands(context);

            var found = commands.FirstOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            return Task.FromResult(found is null
                ? TypeParserResult<CustomCommand>.Unsuccessful("Failed to find command")
                : TypeParserResult<CustomCommand>.Successful(found));
        }
    }
}
