using Discord;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
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

            var response = provider.GetService<ResponseService>();
            var user = context.Invoker;

            return TypeParserResult<Emote>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
        }
    }
}
