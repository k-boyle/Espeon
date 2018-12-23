using Espeon.Attributes;
using Espeon.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    [Service(ServiceLifetime.Singleton)]
    public class CandyService
    {
        [Inject] private readonly DatabaseService _database;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        private async Task UpdateCandiesAsync(ulong id, int amount)
        {
            var user = await _database.GetEntityAsync<User>("users", id);
            user.Candies.Amount += amount;

            if (user.Candies.Highest < user.Candies.Amount)
                user.Candies.Highest = user.Candies.Amount;

            await _database.WriteEntityAsync("users", user);
        }

        public async Task ClaimCandiesAsync(ulong id)
        {
            var user = await _database.GetEntityAsync<User>("users", id);
            user.Candies.Amount += Random.Next(1, 11);

            if (user.Candies.Highest < user.Candies.Amount)
                user.Candies.Highest = user.Candies.Amount;

            user.Candies.LastClaimed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await _database.WriteEntityAsync("users", user);
        }

        public async Task<int> GetCandiesAsync(ulong id)
        {
            var user = await _database.GetEntityAsync<User>("users", id);
            return user.Candies.Amount;
        }

        public async Task<bool> CanClaimCandiesAsync(ulong id)
        {
            var user = await _database.GetEntityAsync<User>("users", id);
            var lastClaimed = DateTimeOffset.FromUnixTimeMilliseconds(user.Candies.LastClaimed);

            return DateTimeOffset.UtcNow - lastClaimed > TimeSpan.FromHours(8);
        }
    }
}
