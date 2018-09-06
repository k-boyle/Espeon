using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Umbreon.Callbacks;

namespace Umbreon.Commands.Modules
{
    public partial class PokemonCommands
    {
        private readonly Random _random;

        [Command("search")]
        [Name("Search")]
        [Summary("Search for a pokemon")]
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
            var encounter = available.FirstOrDefault(x => x.Id == availableList[_random.Next(availableList.Count)]);
            var enc = new Encounter(Context, encounter, player, Services);
            await enc.SetupAsync();
        }
    }
}
