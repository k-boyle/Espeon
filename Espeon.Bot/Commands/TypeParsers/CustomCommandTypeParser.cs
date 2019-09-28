using Espeon.Commands;
using Espeon.Databases;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class CustomCommandTypeParser : EspeonTypeParser<CustomCommand>
    {
        public override async ValueTask<TypeParserResult<CustomCommand>> ParseAsync(Parameter param, string value, EspeonContext context,
            IServiceProvider provider)
        {
            var service = provider.GetService<ICustomCommandsService>();
            var commands = await service.GetCommandsAsync(context);

            var found = commands.FirstOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            if (!(found is null))
                return TypeParserResult<CustomCommand>.Successful(found);

            var response = provider.GetService<IResponseService>();
            var user = context.Invoker;

            return TypeParserResult<CustomCommand>.Unsuccessful(
                response.GetResponse(this, user.ResponsePack, 0));
        }
    }
}
