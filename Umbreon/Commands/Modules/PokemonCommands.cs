using System;
using Discord;
using Discord.Commands;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Callbacks;
using Umbreon.Commands.ModuleBases;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Extensions;
using Umbreon.Services;

namespace Umbreon.Commands.Modules
{
    public class PokemonCommands : UmbreonBase
    {
        private readonly PokemonDataService _data;
        private readonly PokemonPlayerService _player;
        private const string BaseUrl = "https://bulbapedia.bulbagarden.net/wiki/";
        
        public PokemonCommands(PokemonDataService data, PokemonPlayerService player)
        {
            _data = data;
            _player = player;
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
            var image = PokemonDataService.GetImage(pokemon);
            var evolutionData = _data.GetEvolutions(pokemon).ToImmutableArray();
            var builder = new EmbedBuilder
            {
                Title = $"{pokemon.Identifier.FirstLetterToUpper()}",
                Color = _data.GetColour(pokemon),
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

        [Command("travel")]
        [Name("Travel")]
        [Summary("Move to a different area")]
        [Usage("travel")]
        public async Task Travel()
        {
            if (_player.GetTravel(Context.User.Id).ToUniversalTime().AddMinutes(10) > DateTime.UtcNow)
            {
                await SendMessageAsync($"You can only travel once every 10 minutes. You can travel in {(_player.GetTravel(Context.User.Id).ToUniversalTime().AddMinutes(10) - DateTime.UtcNow).Humanize()}");
                return;
            }

            var map = new TravelMenu(Context, Services);
            await map.DisplayAsync();
        }
    }
}
