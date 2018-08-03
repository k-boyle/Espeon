using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Extensions;
using Umbreon.Results;

namespace Umbreon.TypeReaders
{
    public class ModuleInfoTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var commands = services.GetService<CommandService>();
            var modules = commands.Modules;
            var targetModule = modules.FirstOrDefault(x => string.Equals(x.Name, input, StringComparison.CurrentCultureIgnoreCase));
            return targetModule is null
                ? TypeReaderResult.FromError(new FailedResult("Module not found", false,
                    CommandError.ObjectNotFound))
                : (await targetModule.CheckPermissionsAsync(context, services)).IsSuccess
                    ? TypeReaderResult.FromSuccess(targetModule) 
                    : TypeReaderResult.FromError(new FailedResult("You do not have permission to use this module", false, CommandError.UnmetPrecondition));
        }
    }
}
