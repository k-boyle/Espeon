using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class CommandsTypeParser : EspeonTypeParser<IReadOnlyCollection<Command>>
    {
        public override async ValueTask<TypeParserResult<IReadOnlyCollection<Command>>> ParseAsync(Parameter param, string value, EspeonContext context, IServiceProvider provider)
        {
            var service = provider.GetService<CommandService>();
            var commands = service.GetAllCommands();

            var found = commands.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)
                || x.FullAliases.Any(y => y.Contains(value, StringComparison.InvariantCultureIgnoreCase))).ToArray();

            var canExecute = new List<Command>();

            foreach (var command in found)
            {
                var result = await command.RunChecksAsync(context);

                if (result.IsSuccessful)
                {
                    canExecute.Add(command);
                }
            }

            if (canExecute.Count > 0)
                return new TypeParserResult<IReadOnlyCollection<Command>>(canExecute);

            var response = provider.GetService<IResponseService>();
            var user = context.Invoker;

            return new TypeParserResult<IReadOnlyCollection<Command>>(
                response.GetResponse(this, user.ResponsePack, 0, value));
        }
    }
}
