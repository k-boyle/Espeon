using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    [DontOverride]
    public class CandyTypeParser : TypeParser<int>
    {
        public override async ValueTask<TypeParserResult<int>> ParseAsync(Parameter param, string value, CommandContext originalContext, IServiceProvider services)
        {
            var context = (EspeonContext)originalContext;

            var candy = services.GetService<CandyService>();
            var userAmount = await candy.GetCandiesAsync(context, context.User);

            var resp = new Dictionary<ResponsePack, string[]>
            {
                [ResponsePack.Default] = new []
                {
                    "Amount specified is not a valid integer",
                    "Amount specified must be a positive integer",
                    "You don't have enough candies"
                },
                [ResponsePack.owo] = new []
                {
                    "awmount spwecifed is not an intwiger",
                    "awmount spwecified must be pwositive",
                    "ownnoo >.< u dont hav enuff cwandies"
                }
            };

            var nones = new[] { "NaN", "none", "nothing", "zero", "zilch", "nada" };
            var p = context.Invoker.ResponsePack;

            if (nones.Any(x => string.Equals(x, value, StringComparison.InvariantCultureIgnoreCase)))
                return TypeParserResult<int>.Successful(0);

            if (string.Equals(value, "all", StringComparison.InvariantCultureIgnoreCase))
                return TypeParserResult<int>.Successful(userAmount);

            if (!int.TryParse(value, out var amount))
                return TypeParserResult<int>.Unsuccessful(resp[p][0]);
            if (amount < 0)
                return TypeParserResult<int>.Unsuccessful(resp[p][1]);

            return amount > userAmount
                ? TypeParserResult<int>.Unsuccessful(resp[p][2])
                : TypeParserResult<int>.Successful(amount);
        }
    }
}
