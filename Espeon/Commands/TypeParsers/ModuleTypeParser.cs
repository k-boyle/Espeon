using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class ModuleTypeParser : TypeParser<Module>
    {
        public override async ValueTask<TypeParserResult<Module>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = (EspeonContext) ctx;

            var commands = provider.GetService<CommandService>();

            var modules = commands.GetAllModules();
            var module = modules.SingleOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            var p = context.Invoker.ResponsePack;
            var response = provider.GetService<ResponseService>();

            if (module is null)
            {
                var isGuild = string.Equals(value, context.Guild.Name, StringComparison.InvariantCultureIgnoreCase);

                if(!isGuild)
                    return new TypeParserResult<Module>(response.GetResponse(this, p, 0, value));

                module = modules.Single(x => x.Name == context.Guild.Id.ToString());
            }

            var result = await module.RunChecksAsync(context, provider);

            return result.IsSuccessful 
                ? new TypeParserResult<Module>(module) 
                : new TypeParserResult<Module>(response.GetResponse(this, p, 1));
        }
    }
}
