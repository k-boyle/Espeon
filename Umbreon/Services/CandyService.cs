using System;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core.Entities.User;

namespace Umbreon.Services
{
    [Service]
    public class CandyService
    {
        private readonly DatabaseService _database;

        public CandyService(DatabaseService database)
        {
            _database = database;
        }

        public async Task UpdateCandiesAsync(ulong id, bool isClaim, int amount)
        {
            var user = await _database.GetObjectAsync<UserObject>("users", id);
            user.RareCandies += amount;
            if (isClaim)
                user.LastClaimed = DateTime.UtcNow;
            _database.UpdateObject("users", user);
        }

        public async Task SetCandiesAsync(ulong id, int amount)
        {
            var user = await _database.GetObjectAsync<UserObject>("users", id);
            user.RareCandies = amount;
            _database.UpdateObject("users", user);
        }

        public async Task<bool> CanClaimAsync(ulong id)
        {
            var user = await _database.GetObjectAsync<UserObject>("users", id);
            return DateTime.UtcNow - user.LastClaimed > TimeSpan.FromHours(8);
        }

        public async Task<int> GetCandiesAsync(ulong id)
            => (await _database.GetObjectAsync<UserObject>("users", id)).RareCandies;
    }
}
