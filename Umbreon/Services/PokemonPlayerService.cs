using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Entities.Pokemon;
using Umbreon.Core.Entities.Pokemon.Pokeballs;
using Umbreon.Core.Entities.User;

namespace Umbreon.Services
{
    [Service]
    public class PokemonPlayerService
    {
        private readonly DatabaseService _database;
        private readonly LogService _log;

        private readonly ConcurrentDictionary<ulong, PlayingData> _data = new ConcurrentDictionary<ulong, PlayingData>();
        private readonly ConcurrentDictionary<ulong, bool> _inEncounter = new ConcurrentDictionary<ulong, bool>();

        private readonly IReadOnlyDictionary<int, string> _habitats = new Dictionary<int, string>
        {
            { 1, "Cave" },
            { 2, "Forest" },
            { 3, "Grassland" },
            { 4, "Mountain" },
            { 5, "Rare" },
            { 6, "Rough-terrain" },
            { 7, "Sea" },
            { 8, "Urban" },
            { 9, "Water-edge" },
            { 10, "-" }
        };

        private readonly IReadOnlyDictionary<int, string> _habitatImages = new Dictionary<int, string>
        {
            { 1, "https://cdn.bulbagarden.net/upload/8/85/HGSS_Union_Cave-Day.png" },
            { 2, "https://cdn.bulbagarden.net/upload/8/8c/HGSS_Viridian_Forest-Day.png" },
            { 3, "http://orig15.deviantart.net/514e/f/2014/298/5/1/pokemon_x_and_y_battle_background_11_by_phoenixoflight92-d843okx.png" },
            { 4, "https://cdn.bulbagarden.net/upload/8/83/Twist_Mountain_anime.png" },
            { 5, "https://vignette.wikia.nocookie.net/pokemon-fighters-ex-roblox/images/a/a2/Rare_Candy.png/revision/latest?cb=20180410175601" },
            { 6, "https://pm1.narvii.com/5874/57112bd7b5ad3bc1c02a010422fab2819ccfbf59_hq.jpg" },
            { 7, "https://cdn.bulbagarden.net/upload/1/19/HGSS_Whirl_Islands-Day.png" },
            { 8, "https://i.imgur.com/8R1Iv7j.png" },
            { 9, "https://pm1.narvii.com/6160/772d9919f79ccac25925e822170d9273ab5fcda8_hq.jpg" },
            { 10, "-" }
        };

        public PokemonPlayerService(DatabaseService database, LogService log)
        {
            _database = database;
            _log = log;
        }

        public void Initialise()
        {
            foreach (var user in DatabaseService.GrabAllData<UserObject>("users"))
                _data.TryAdd(user.Id, user.Data);
            _log.NewLogEvent(LogSeverity.Info, LogSource.Pokemon, "All player data loaded into cache");
        }

        public IReadOnlyDictionary<int, string> GetHabitats()
            => _habitats;

        public void SetArea(ulong id, int area)
        {
            var newData = new PlayingData
            {
                LastMoved = DateTime.UtcNow,
                Location = area
            };

            _data[id] = newData;

            var user = _database.GetObject<UserObject>("users", id);
            user.Data = newData;
            _database.UpdateObject("users", user);
        }

        public int GetHabitat(ulong id)
            => _data.TryGetValue(id, out var player) ? player.Location : 0;

        public DateTime GetTravel(ulong id)
            => _data.TryGetValue(id, out var player) ? player.LastMoved : DateTime.UtcNow.AddMinutes(-10);

        public string GetImageUrl(int id)
            => _habitatImages[id];

        public UserObject GetCurrentPlayer(ulong id)
            => _database.GetObject<UserObject>("users", id);

        public void UseBall(UserObject user, BaseBall ball)
        {
            user.Bag.PokeBalls.Remove(ball);
            _database.UpdateObject("users", user);
        }

        public void AddItem(ulong userId, ShopItemAttribute item)
        {
            var user = GetCurrentPlayer(userId);
            switch (item.ItemName)
            {
                case "Pokeball":
                    user.Bag.PokeBalls.Add(new NormalBall());
                    break;

                case "Greatball":
                    user.Bag.PokeBalls.Add(new GreatBall());
                    break;

                case "Ultraball":
                    user.Bag.PokeBalls.Add(new UltraBall());
                    break;

                case "Masterball":
                    user.Bag.PokeBalls.Add(new MasterBall());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(item));
            }

            _database.UpdateObject("users", user);
        }

        public void UpdateDexEntry(UserObject user, PokemonData pokemon)
        {
            var entry = user.Pokedex.FirstOrDefault(x => x.Id == pokemon.Id);
            if(entry is null)
            {
                user.Pokedex.Add(new PokedexEntry
                {
                    Id = pokemon.Id,
                    Caught = true,
                    Count = 1
                });
            }
            else
            {
                user.Pokedex[user.Pokedex.IndexOf(entry)].Count++;
            }

            _database.UpdateObject("users", user);
        }

        public void SetEncounter(ulong id, bool encounter)
            => _inEncounter[id] = encounter;

        public bool InEncounter(ulong id)
            => _inEncounter.TryGetValue(id, out var res) && res;
    }
}
