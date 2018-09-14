using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Umbreon.Commands.TypeReaders
{
    public class VolumeTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            if (!uint.TryParse(input, out var vol))
                return Task.FromResult(TypeReaderResult.FromError(command, CommandError.ParseFailed,
                    "Failed to parse integer volume"));
            return Task.FromResult(vol <= 100
                ? TypeReaderResult.FromSuccess(command, (uint)Math.Floor(vol * 1.5))
                : TypeReaderResult.FromError(command, CommandError.ParseFailed, "Volume must be between 0 - 100"));
        }
    }
}
