using Casino.Common.Qmmands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    [DontOverride]
    public class CandyTypeParser : TypeParser<int>
    {
        public override async ValueTask<TypeParserResult<int>> ParseAsync(Parameter param, string value, CommandContext originalContext, IServiceProvider provider)
        {
            var context = (EspeonContext)originalContext;

            var candy = provider.GetService<CandyService>();
            var userAmount = await candy.GetCandiesAsync(context, context.User);

            var nones = new[] { "NaN", "none", "nothing", "zero", "zilch", "nada" };
            var p = context.Invoker.ResponsePack;
            var response = provider.GetService<ResponseService>();

            if (nones.Any(x => string.Equals(x, value, StringComparison.InvariantCultureIgnoreCase)))
                return TypeParserResult<int>.Successful(0);

            if (string.Equals(value, "all", StringComparison.InvariantCultureIgnoreCase))
                return TypeParserResult<int>.Successful(userAmount);

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
