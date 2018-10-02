using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Core.Entities.Guild;
using Espeon.Services;

namespace Espeon.Commands.TypeReaders
{
    [TypeReader(typeof(CustomCommand))]
    public class CustomCommandTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var cmdService = services.GetService<CustomCommandsService>();
            var currentCmds = await cmdService.GetCmdsAsync(context);
            return CustomCommandsService.TryParse(currentCmds, input, out var foundCmd) ? 
                TypeReaderResult.FromSuccess(command, foundCmd) : 
                TypeReaderResult.FromError(command, CommandError.ParseFailed, "Custom command not found");
        }
    }
}
