using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class ModuleTypeParser : TypeParser<Module>
    {
        public override async ValueTask<TypeParserResult<Module>> ParseAsync(Parameter param, string value, CommandContext context, IServiceProvider provider)
        {
            var commands = provider.GetService<CommandService>();

            var module = commands.GetAllModules().SingleOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            if (module is null)
                return new TypeParserResult<Module>($"Failed to find module with name {value}");

            var result = await module.RunChecksAsync(context, provider);

            return result.IsSuccessful 
                ? new TypeParserResult<Module>(module) 
                : new TypeParserResult<Module>("You lack the required permissions to view this module");
        }
    }
}
