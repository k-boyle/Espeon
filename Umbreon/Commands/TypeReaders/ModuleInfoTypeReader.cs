using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Commands.Preconditions;
using Umbreon.Extensions;

namespace Umbreon.Commands.TypeReaders
{
    public class ModuleInfoTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var service = services.GetService<CommandService>();
            var modules = service.Modules;
            var found = modules.FirstOrDefault(x =>
                string.Equals(x.Name, input, StringComparison.CurrentCultureIgnoreCase));

            if(found is null)
                return TypeReaderResult.FromError(new FailedResult("Module not found", CommandError.Unsuccessful));

            if(!(await found.CheckPermissionsAsync(context, services)).IsSuccess)
                return TypeReaderResult.FromError(new FailedResult("You cann't execute any commands in this module", CommandError.Unsuccessful));

            return found.Name == "Help" ? TypeReaderResult.FromError(new FailedResult("Module not found", CommandError.Unsuccessful)) : TypeReaderResult.FromSuccess(found);
        }
    }
}
