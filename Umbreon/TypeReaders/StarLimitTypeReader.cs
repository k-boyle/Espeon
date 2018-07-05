using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Umbreon.TypeReaders
{
    public class StarLimitTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (!int.TryParse(input, out var num))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                    "Integer greater than 0 input required"));
            return Task.FromResult(num > 0 ? TypeReaderResult.FromSuccess(num) : TypeReaderResult.FromError(CommandError.ParseFailed, "Integer greater than 0 input required"));
        }
    }
}
