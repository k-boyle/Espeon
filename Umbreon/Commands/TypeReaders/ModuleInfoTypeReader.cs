using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Extensions;

namespace Umbreon.Commands.TypeReaders
{
    [TypeReader(typeof(ModuleInfo))]
    public class ModuleInfoTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var service = services.GetService<CommandService>();
            var modules = service.Modules;
            var found = modules.FirstOrDefault(x =>
                string.Equals(x.Name, input, StringComparison.CurrentCultureIgnoreCase));

            if(found is null)
                return TypeReaderResult.FromError(command, CommandError.ParseFailed, "Module not found");

            if(!(await found.CheckPermissionsAsync(context, services)).IsSuccess)
                return TypeReaderResult.FromError(command, CommandError.Unsuccessful, "You can't execute any commands in this module");

            return found.Name == "Help" ? TypeReaderResult.FromError(command, CommandError.ParseFailed, "Module not found") : TypeReaderResult.FromSuccess(command, found);
        }
    }
}
