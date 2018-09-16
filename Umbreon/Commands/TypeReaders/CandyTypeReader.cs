using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Services;

namespace Umbreon.Commands.TypeReaders
{
    public class CandyTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var candy = services.GetService<CandyService>();
            var user = await candy.GetCandiesAsync(context.User.Id);

            if (string.Equals(input, "NaN")) //JS meme
                return TypeReaderResult.FromSuccess(command, 0);

            if (string.Equals(input, "all", StringComparison.CurrentCultureIgnoreCase))
                return TypeReaderResult.FromSuccess(command, user);

            if (!int.TryParse(input, out var amount))
                return TypeReaderResult.FromError(command, CommandError.ParseFailed, "Failed to parse amount");
            if (amount < 0)
                return TypeReaderResult.FromError(command, CommandError.ParseFailed, "Amount must be a positive integer");

            return amount > user ? TypeReaderResult.FromError(command, CommandError.Unsuccessful, "You don't have enough candies") : TypeReaderResult.FromSuccess(command, amount);
        }
    }
}
