using System;
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

        public void UpdateCandies(ulong id, bool isClaim, int amount)
        {
            var user = _database.GetObject<UserObject>("users", id);
            user.RareCandies += amount;
            if (isClaim)
                user.LastClaimed = DateTime.UtcNow;
            _database.UpdateObject("users", user);
        }

        public void SetCandies(ulong id, int amount)
        {
            var user = _database.GetObject<UserObject>("users", id);
            user.RareCandies = amount;
            _database.UpdateObject("users", user);
        }

        public bool CanClaim(ulong id)
        {
            var user = _database.GetObject<UserObject>("users", id);
            return DateTime.UtcNow - user.LastClaimed > TimeSpan.FromHours(8);
        }

        public int GetCandies(ulong id)
            => _database.GetObject<UserObject>("users", id).RareCandies;
    }
}
