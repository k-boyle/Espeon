using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Services;

namespace Umbreon.TypeReaders
{
    public class CustomCommandTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var cmdService = services.GetService<CustomCommandsService>();
            var currentCmds = cmdService.GetCmds(context);
            return Task.FromResult(CustomCommandsService.TryParse(currentCmds, input, out var foundCmd) ? TypeReaderResult.FromSuccess(foundCmd) : TypeReaderResult.FromError(CommandError.ParseFailed, "Custom command not found"));
        }
    }
}
