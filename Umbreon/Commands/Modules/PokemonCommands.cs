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
using Colour = Discord.Color;

namespace Umbreon.Commands.Modules
{
    [Name("Pokemon Commands")]
    [Summary("Commands that allow you to play Umbreon Go")]
    public partial class PokemonCommands : UmbreonBase
    {
        private readonly PokemonDataService _data;
        private readonly PokemonPlayerService _player;
        private readonly CandyService _candy;
        private const string BaseUrl = "https://bulbapedia.bulbagarden.net/wiki/";

        public PokemonCommands(PokemonDataService data, PokemonPlayerService player, CandyService candy, Random random)
        {
            _data = data;
            _player = player;
            _candy = candy;
            _random = random;
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
                Title = $"{pokemon.Name.FirstLetterToUpper()}",
                Color = _data.GetColour(pokemon),
                ThumbnailUrl = $"{(image is null ? "" : "attachment://image.png")}",
                Url = $"{BaseUrl}{pokemon.Name}_(Pokémon)",
                Description = $"Catch rate: {pokemon.CaptureRate}\n" +
                              $"Appearance rate: {pokemon.EncounterRate}\n" +
                              $"Habitat: {_player.GetHabitats()[pokemon.HabitatId ?? 10]}"
            };

            if (evolutionData.Length > 0)
            {
                builder.AddField("Evolution Chain:", string.Join("\n", evolutionData.Select(x => $"{x.Key.Name.FirstLetterToUpper()} \t({x.Key.Id.ToString().PadLeft(3, '0')}) \t{(x.Value == 0 ? "" : $"at level {x.Value}")}")));
            }

            await SendFileAsync(image, string.Empty, embed: builder.Build());
        }

        [Command("travel")]
        [Name("Travel")]
        [Summary("Travel to a different area")]
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

        [Command("travel")]
        [Name("travel")]
        [Summary("Travel to a different area")]
        [Usage("travel 1")]
        public async Task Travel(
            [Name("Habitat")]
            [Summary("The habitat you want to travel to")]
            [Remainder] Habitat habitat)
        {
            if (_player.GetTravel(Context.User.Id).ToUniversalTime().AddMinutes(10) > DateTime.UtcNow)
            {
                await SendMessageAsync($"You can only travel once every 10 minutes. You can travel in {(_player.GetTravel(Context.User.Id).ToUniversalTime().AddMinutes(10) - DateTime.UtcNow).Humanize()}");
                return;
            }

            if (habitat.Id == _player.GetHabitat(Context.User.Id))
            {
                await SendMessageAsync("You are already in this area");
                return;
            }

            if (habitat.Id == 5)
            {
                if (_candy.GetCandies(Context.User.Id) < 10)
                {
                    await SendMessageAsync("You don't have enough candies to enter this zone");
                    return;
                }
            }

            _player.SetArea(Context.User.Id, habitat.Id);
            await SendMessageAsync($"You have traveled to {habitat.Name} area");
        }

        [Command("area")]
        [Name("Area Info")]
        [Summary("Get information on an area")]
        [Usage("area 1")]
        public async Task GetArea(
            [Name("Habitat")]
            [Summary("The habitat you want to get info on")]
            [Remainder] Habitat habitat)
        {
            var builder = new EmbedBuilder
            {
                Title = habitat.Name,
                Color = Colour.DarkPurple,
                ThumbnailUrl = habitat.ImageUrl
            };

            builder.AddField("Available Pokemon", string.Join(", ", habitat.Pokemon.Select(x => $"`{x.Name.FirstLetterToUpper()}`")));

            await SendMessageAsync(string.Empty, embed: builder.Build());
        }

        [Command("areas")]
        [Name("List Areas")]
        [Summary("See all the areas")]
        [Usage("areas")]
        public Task Areas()
            => SendMessageAsync(string.Join("\n", _player.GetHabitats().Select(x => $"{x.Key}: {x.Value}").ToArray(), 0, 9));
    }
}
