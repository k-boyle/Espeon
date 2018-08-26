using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Umbreon.Commands.TypeReaders
{
    public class BanLimitTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (!uint.TryParse(input, out var num))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                    "Prune amount must be an integer input > 0"));
            if (num > 7)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                    "Prune amount cannot be > 7"));
            return Task.FromResult(TypeReaderResult.FromSuccess((int)num));
        }
    }
}
