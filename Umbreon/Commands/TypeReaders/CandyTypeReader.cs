using Discord.Commands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Commands.Preconditions;
using Umbreon.Services;

namespace Umbreon.Commands.TypeReaders
{
    public class CandyTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var candy = services.GetService<CandyService>();
            var user = candy.GetCandies(context.User.Id);

            if (string.Equals(input, "all", StringComparison.CurrentCultureIgnoreCase))
                return Task.FromResult(TypeReaderResult.FromSuccess(user));

            if (!int.TryParse(input, out var amount))
                return Task.FromResult(
                    TypeReaderResult.FromError(new FailedResult("Failed to parse amount", CommandError.ParseFailed)));
            if (amount < 0)
                return Task.FromResult(TypeReaderResult.FromError(new FailedResult("Requires a positive integer", CommandError.Unsuccessful)));

            if (amount > user)
                return Task.FromResult(TypeReaderResult.FromError(new FailedResult("You don't have enough candies",
                    CommandError.Unsuccessful)));

            return Task.FromResult(TypeReaderResult.FromSuccess(amount));

        }
    }
}
