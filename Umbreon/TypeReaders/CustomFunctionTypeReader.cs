using Discord.Commands;
using System;
using System.Threading.Tasks;
using Umbreon.Extensions;

namespace Umbreon.TypeReaders
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
