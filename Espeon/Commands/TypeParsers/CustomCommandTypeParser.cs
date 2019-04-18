using Espeon.Databases;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class CustomCommandTypeParser : TypeParser<CustomCommand>
    {
        public override async ValueTask<TypeParserResult<CustomCommand>> ParseAsync(Parameter param, string value, CommandContext originalContext,
            IServiceProvider provider)
        {
            var context = (EspeonContext)originalContext;

            var service = provider.GetService<CustomCommandsService>();
            var commands = await service.GetCommandsAsync(context);

            var found = commands.FirstOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            if (found is null)
            {
                var resp = new Dictionary<ResponsePack, string>
                {
                    [ResponsePack.Default] = "Failed to find a matching command",
                    [ResponsePack.owo] = "fwailed to fwind cwommand"
                };

                return TypeParserResult<CustomCommand>.Unsuccessful(resp[context.Invoker.ResponsePack]);
            }

            return TypeParserResult<CustomCommand>.Successful(found);
        }
    }
}
