using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Umbreon.TypeReaders
{
    public class StarLimitTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            return Task.FromResult(uint.TryParse(input, out var num) ? TypeReaderResult.FromSuccess((int)num) : TypeReaderResult.FromError(CommandError.ParseFailed, "Requires an integer input > 0"));
        }
    }
}
