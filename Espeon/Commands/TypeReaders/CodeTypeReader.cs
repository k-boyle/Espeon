using Discord.Commands;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Espeon.Extensions;

namespace Espeon.Commands.TypeReaders
{
    public class CodeTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var foundCodes = input.GetCodes().ToImmutableArray();
            return Task.FromResult(TypeReaderResult.FromSuccess(command, foundCodes.Length > 0 ? string.Join("\n", foundCodes) : input));
        }
    }
}
