using System;
using System.Threading.Tasks;
using Discord.Commands;
using Umbreon.Extensions;

namespace Umbreon.Commands.TypeReaders
{
    public class CustomFunctionTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var func = input.GetFunction();
            if (func.GuildId != 0)
                func.GuildId = context.Guild.Id;

            return Task.FromResult(func is null
                ? TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse command")
                : TypeReaderResult.FromSuccess(func));
        }
    }
}
