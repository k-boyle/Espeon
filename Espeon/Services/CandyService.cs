using Espeon.Core.Attributes;
using Espeon.Core.Services;
using Espeon.Entities;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Espeon.Services
{
    [Service(typeof(ICandyService), ServiceLifetime.Singleton, true)]
    public class CandyService : ICandyService
    {
        [Inject] private readonly IDatabaseService _database;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        Task ICandyService.AddCandiesAsync(ulong id, int amount)
            => UpdateCandiesAsync(id, amount);

        Task ICandyService.RemoveCandiesAsync(ulong id, int amount)
            => UpdateCandiesAsync(id, amount);

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
