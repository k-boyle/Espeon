using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public class EmoteTypeParser : TypeParser<Emote>
    {
        public override ValueTask<TypeParserResult<Emote>> ParseAsync(Parameter parameter, string value, CommandContext context, IServiceProvider provider)
        {
            return new ValueTask<TypeParserResult<Emote>>(
                Emote.TryParse(value, out var emote)
                ? TypeParserResult<Emote>.Successful(emote)
                : TypeParserResult<Emote>.Unsuccessful("Failed to parse emote"));
        }
    }
}
