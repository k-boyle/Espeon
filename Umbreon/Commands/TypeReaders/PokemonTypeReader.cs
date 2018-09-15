using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Helpers;
using Umbreon.Services;

namespace Umbreon.Commands.TypeReaders
{
    [TypeReader(typeof(PokemonData))]
    public class PokemonTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var pokemonService = services.GetService<PokemonDataService>();
            var data = int.TryParse(input, out var id) ? pokemonService.GetData(id) : pokemonService.GetData(input);
            return Task.FromResult(data.Id > ConstantsHelper.PokemonLimit ? 
                TypeReaderResult.FromError(command, CommandError.ParseFailed, "This pokemon is not part of the dataset") : 
                TypeReaderResult.FromSuccess(command, data));
        }
    }
}
