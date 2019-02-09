using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    [DontAdd]
    public class CandyTypeParser : TypeParser<int>
    {
        public override async Task<TypeParserResult<int>> ParseAsync(string value, ICommandContext originalContext, IServiceProvider services)
        {
            var context = originalContext as EspeonContext;

            var candy = services.GetService<CandyService>();
            var userAmount = await candy.GetCandiesAsync(context, context!.User.Id);

            if (string.Equals(value, "NaN")) //JS meme
                return TypeParserResult<int>.Successful(0);

            if (string.Equals(value, "all", StringComparison.CurrentCultureIgnoreCase))
                return TypeParserResult<int>.Successful(userAmount);

            if (!int.TryParse(value, out var amount))
                return TypeParserResult<int>.Unsuccessful("Amount specified is not a valid integer");
            if (amount < 0)
                return TypeParserResult<int>.Unsuccessful("Amount specified must be a positive integer");

            return amount > userAmount
                ? TypeParserResult<int>.Unsuccessful("You don't have enough candies")
                : TypeParserResult<int>.Successful(amount);
        }
    }
}
