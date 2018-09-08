using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Umbreon.Attributes;
using Umbreon.Callbacks;
using Umbreon.Commands.Preconditions;

namespace Umbreon.Commands.Modules
{
    public partial class PokemonCommands
    {
        private readonly Random _random;

        [Command("search")]
        [Name("Search")]
        [Summary("Search for a pokemon")]
        [Usage("search")]
        [RequireEncounter]
        public async Task Search()
        {
            var player = _player.GetCurrentPlayer(Context.User.Id);
            if (player.Bag.PokeBalls.Count == 0)
            {
                await SendMessageAsync("You don't have any pokeballs! Visit the shop");
                return;
            }

            var available = _data.GetAllData().Where(x => x.HabitatId == player.Data.Location && x.EncounterRate > 0).ToImmutableList();
            var availableList = new List<int>();
            foreach (var pokemon in available)
                for (var i = 0; i < pokemon.EncounterRate; i++)
                    availableList.Add(pokemon.Id);
            var ran = _random.Next(availableList.Count);
            var encounter = available.FirstOrDefault(x => x.Id == availableList[ran]);
            var enc = new Encounter(Context, encounter, Context.User.Id, Services);
            await enc.SetupAsync();
        }

        [Command("shop")]
        [Name("Shop")]
        [Summary("View the buyable items")]
        [Usage("shop")]
        public async Task Shop()
        {
            var shop = new Shop(Context, Services);
            await shop.DisplayAsync();
        }
    }
}
