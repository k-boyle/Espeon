using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Commands.Preconditions;
using Umbreon.Helpers;
using Umbreon.Services;

namespace Umbreon.Commands.TypeReaders
{
    public class PokemonTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var pokemonService = services.GetService<PokemonService>();
            var data = int.TryParse(input, out var id) ? pokemonService.GetData(id) : pokemonService.GetData(input);
            if (data.Id > ConstantsHelper.PokemonLimit)
                return Task.FromResult(
                    TypeReaderResult.FromError(new FailedResult("This pokemon is not part of the dataset", false, CommandError.ParseFailed)));
            return Task.FromResult(!(data is null)
                ? TypeReaderResult.FromSuccess(data)
                : TypeReaderResult.FromError(
                    new FailedResult("Failed to find pokemon", false, CommandError.ParseFailed)));
        }
    }
}
