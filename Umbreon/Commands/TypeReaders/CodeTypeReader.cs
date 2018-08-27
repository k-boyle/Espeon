using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Umbreon.Extensions;

namespace Umbreon.Commands.TypeReaders
{
    public class CodeTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var foundCodes = input.GetCodes();
            return Task.FromResult(TypeReaderResult.FromSuccess(foundCodes.Count() > 0 ? string.Join("\n", foundCodes) : input));
        }
    }
}
