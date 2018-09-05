using Discord;
using Discord.Commands;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.ModuleBases;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Extensions;
using Umbreon.Services;

namespace Umbreon.Commands.Modules
{
    public class PokemonCommands : UmbreonBase
    {
        private readonly PokemonService _pokemon;
        private const string BaseUrl = "https://bulbapedia.bulbagarden.net/wiki/";

        public PokemonCommands(PokemonService pokemon)
        {
            _pokemon = pokemon;
        }

        [Command("data")]
        [Name("Pokemon Data")]
        [Summary("View the data of a pokemon")]
        [Usage("data 1")]
        public async Task ViewData(
            [Name("Identifier")]
            [Summary("The name/id of the pokemon you want data on")]
            [Remainder] PokemonData pokemon)
        {
            var image = PokemonService.GetImage(pokemon);
            var evolutionData = _pokemon.GetEvolutions(pokemon).ToImmutableArray();
            var builder = new EmbedBuilder
            {
                Title = $"{pokemon.Identifier.FirstLetterToUpper()}",
                Color = _pokemon.GetColour(pokemon),
                ThumbnailUrl = $"{(image is null ? "" : "attachment://image.png")}",
                Url = $"{BaseUrl}{pokemon.Identifier}_(Pokémon)",
                Description = $"Catch rate: {pokemon.CaptureRate}\n" +
                              $"Appearance rate: {pokemon.CaptureRate / (float)100}"
            };

            if (evolutionData.Length > 0)
            {
                builder.AddField("Evolution Chain:", string.Join("\n", evolutionData.Select(x => $"{x.Key.Identifier.FirstLetterToUpper()} \t({x.Key.Id.ToString().PadLeft(3, '0')}) \t{(x.Value == 0 ? "" : $"at level {x.Value}")}")));
            }

            await Context.Channel.SendFileAsync(image, "image.png", string.Empty, embed: builder.Build());
        }
    }
}
