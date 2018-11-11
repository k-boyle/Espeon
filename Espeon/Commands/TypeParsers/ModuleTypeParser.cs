using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public class ModuleTypeParser : TypeParser<Module>
    {
        public override Task<TypeParserResult<Module>> ParseAsync(string value, ICommandContext context, IServiceProvider provider)
        {
            var commands = provider.GetService<CommandService>();

            var module = commands.GetAllModules().SingleOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            return Task.FromResult(module is null
                ? new TypeParserResult<Module>($"Failed to find module: {value}")
                : new TypeParserResult<Module>(module));
        }
    }
}
