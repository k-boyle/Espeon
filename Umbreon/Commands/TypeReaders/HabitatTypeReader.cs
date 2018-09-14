using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Services;

namespace Umbreon.Commands.TypeReaders
{
    public class HabitatTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, CommandInfo command, string input, IServiceProvider services)
        {
            var data = services.GetService<PokemonDataService>();
            var player = services.GetService<PokemonPlayerService>();
            var habitats = player.GetHabitats();

            if (int.TryParse(input, out var id))
            {
                if (id < 0 || id > 9)
                    return Task.FromResult(TypeReaderResult.FromError(command, CommandError.ParseFailed, "Area code can only be between 1-9"));
            }
            else
            {
                if (habitats.All(x => !string.Equals(x.Value, input, StringComparison.CurrentCultureIgnoreCase)))
                    return Task.FromResult(TypeReaderResult.FromError(command, CommandError.ParseFailed, "Area not found"));
                id = habitats.FirstOrDefault(x => string.Equals(x.Value, input, StringComparison.CurrentCultureIgnoreCase)).Key;
            }

            var availablePokemon = data.GetAllData().Where(x => x.HabitatId == id && x.EncounterRate > 0);
            return Task.FromResult(TypeReaderResult.FromSuccess(command, new Habitat
            {
                Id = id,
                Name = habitats[id],
                Pokemon = availablePokemon.ToImmutableList(),
                ImageUrl = player.GetImageUrl(id)
            }));
        }
    }
}
