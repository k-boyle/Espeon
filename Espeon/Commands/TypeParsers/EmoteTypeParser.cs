using Discord;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public class EmoteTypeParser : TypeParser<Emote>
    {
        public override ValueTask<TypeParserResult<Emote>> ParseAsync(Parameter parameter, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = (EspeonContext)ctx;

            if (Emote.TryParse(value, out var emote))
                return TypeParserResult<Emote>.Successful(emote);

            var resp = new Dictionary<ResponsePack, string>
            {
                [ResponsePack.Default] = "Failed to parse emote",
                [ResponsePack.owo] = "fwailed to pwarse wemote"
            };

            return TypeParserResult<Emote>.Unsuccessful(resp[context.Invoker.ResponsePack]);
        }
    }
}
