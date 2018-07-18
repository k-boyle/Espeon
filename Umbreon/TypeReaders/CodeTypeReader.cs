using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Helpers;

namespace Umbreon.TypeReaders
{
    public class CodeTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var foundCodes = input.GetCodes();
            return Task.FromResult(TypeReaderResult.FromSuccess(foundCodes.Any() ? string.Join("\n", foundCodes) : input));
        }
    }
}
