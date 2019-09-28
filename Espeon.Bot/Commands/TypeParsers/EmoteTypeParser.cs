using Discord;
using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands.TypeParsers
{
    public class EmoteTypeParser : EspeonTypeParser<Emote>
    {
        public override ValueTask<TypeParserResult<Emote>> ParseAsync(Parameter parameter, string value, EspeonContext context, IServiceProvider provider)
        {
            if (Emote.TryParse(value, out var emote))
                return TypeParserResult<Emote>.Successful(emote);

            var response = provider.GetService<IResponseService>();
            var user = context.Invoker;

            return TypeParserResult<Emote>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
        }
    }
}
