using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Discord;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Entities.User;

namespace Umbreon.Services
{
    [Service]
    public class PokemonPlayerService
    {
        private readonly DatabaseService _database;
        private readonly LogService _log;

        private readonly ConcurrentDictionary<ulong, PlayingData> _data = new ConcurrentDictionary<ulong, PlayingData>();
        private readonly IReadOnlyDictionary<int, string> _habitats = new Dictionary<int, string>
        {
            { 1, "Cave" },
            { 2, "Forest" },
            { 3, "Grassland" },
            { 4, "Mountain" },
            { 5, "Rare" },
            { 6, "Rough-terrain" },
            { 7, "sea" },
            { 8, "Urban" },
            { 9, "Water-edge" }
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
    }
}
