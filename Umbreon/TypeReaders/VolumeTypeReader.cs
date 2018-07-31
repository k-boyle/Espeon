using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Umbreon.TypeReaders
{
    public class VolumeTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (!uint.TryParse(input, out var vol))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                    "Failed to parse integer volume"));
            return Task.FromResult(vol >= 0 && vol <= 100
                ? TypeReaderResult.FromSuccess((uint)Math.Floor(vol * 1.5))
                : TypeReaderResult.FromError(CommandError.ParseFailed, "Volume must be between 0 - 100"));
        }
    }
}
