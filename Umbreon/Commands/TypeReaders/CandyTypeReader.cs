using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Services;

namespace Umbreon.Commands.TypeReaders
{
    public class CandyTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var candy = services.GetService<CandyService>();
            var user = candy.GetCandies(context.User.Id);

            if (string.Equals(input, "NaN")) //JS meme
                return Task.FromResult(TypeReaderResult.FromSuccess(command, 0));

            if (string.Equals(input, "all", StringComparison.CurrentCultureIgnoreCase))
                return Task.FromResult(TypeReaderResult.FromSuccess(command, user));

            if (!int.TryParse(input, out var amount))
                return Task.FromResult(
                    TypeReaderResult.FromError(command, CommandError.ParseFailed, "Failed to parse amount"));
            if (amount < 0)
                return Task.FromResult(TypeReaderResult.FromError(command, CommandError.ParseFailed, "Amount must be a positive integer"));

            if (amount > user)
                return Task.FromResult(TypeReaderResult.FromError(command, CommandError.Unsuccessful, "You don't have enough candies"));

            return Task.FromResult(TypeReaderResult.FromSuccess(command, amount));

        }
    }
}
