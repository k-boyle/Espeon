using Casino.Qmmands;
using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    [DontOverride]
    public class CandyTypeParser : EspeonTypeParser<int>
    {
        public override async ValueTask<TypeParserResult<int>> ParseAsync(Parameter param, string value, EspeonContext context, IServiceProvider provider)
        {
            var candy = provider.GetService<ICandyService>();
            var userAmount = await candy.GetCandiesAsync(context, context.User);

            var nones = new[] { "NaN", "none", "nothing", "zero", "zilch", "nada" };
            var p = context.Invoker.ResponsePack;
            var response = provider.GetService<IResponseService>();

            if (nones.Any(x => string.Equals(x, value, StringComparison.InvariantCultureIgnoreCase)))
                return TypeParserResult<int>.Successful(0);

            if (string.Equals(value, "all", StringComparison.InvariantCultureIgnoreCase))
                return TypeParserResult<int>.Successful(userAmount);

            if(string.Equals(value, "half", StringComparison.InvariantCultureIgnoreCase))
                return TypeParserResult<int>.Successful(userAmount/2);

            if (!int.TryParse(value, out var amount))
                return TypeParserResult<int>.Unsuccessful(response.GetResponse(this, p, 0));
            if (amount < 0)
                return TypeParserResult<int>.Unsuccessful(response.GetResponse(this, p, 1));

            return amount > userAmount
                ? TypeParserResult<int>.Unsuccessful(response.GetResponse(this, p, 2))
                : TypeParserResult<int>.Successful(amount);
        }
    }
}
