using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.TypeReaders
{
    [TypeReader(typeof(CustomCommand))]
    public class CustomCommandTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var cmdService = services.GetService<CustomCommandsService>();
            var currentCmds = cmdService.GetCmds(context);
            return Task.FromResult(CustomCommandsService.TryParse(currentCmds, input, out var foundCmd) ? 
                TypeReaderResult.FromSuccess(command, foundCmd) : 
                TypeReaderResult.FromError(command, CommandError.ParseFailed, "Custom command not found"));
        }
    }
}
