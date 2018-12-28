using Espeon.Attributes;
using Espeon.Commands;
using Espeon.Database;
using Espeon.Database.Entities;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class CandyService : IService
    {
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        public Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
            => Task.CompletedTask;

        private async Task UpdateCandiesAsync(EspeonContext context, ulong id, int amount)
        {
            var user = await context.Database.Users.FindAsync(id) ?? new User
            {
                Id = id
            };

            user.Candies.Amount += amount;

            if (user.Candies.Highest < user.Candies.Amount)
                user.Candies.Highest = user.Candies.Amount;

            await context.Database.Users.UpsertAsync(user);
        }

        public async Task ClaimCandiesAsync(EspeonContext context, ulong id)
        {
            var user = await context.Database.Users.FindAsync(id) ?? new User
            {
                Id = id
            };

            user.Candies.Amount += Random.Next(1, 11);

            if (user.Candies.Highest < user.Candies.Amount)
                user.Candies.Highest = user.Candies.Amount;

            user.Candies.LastClaimed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await context.Database.Users.UpsertAsync(user);
        }

        public async Task<int> GetCandiesAsync(EspeonContext context, ulong id)
        {
            var user = await context.Database.Users.FindAsync(id);

            if (!(user is null)) return user.Candies.Amount;

            user = new User
            {
                Id = id
            };

            await context.Database.Users.AddAsync(user);

            return user.Candies.Amount;
        }

        public async Task<bool> CanClaimCandiesAsync(EspeonContext context, ulong id)
        {
            var user = await context.Database.Users.FindAsync(id);

            if (user is null)
            {
                user = new User
                {
                    Id = id
                };

                await context.Database.AddAsync(user);
            }

            var lastClaimed = DateTimeOffset.FromUnixTimeMilliseconds(user.Candies.LastClaimed);

            return DateTimeOffset.UtcNow - lastClaimed > TimeSpan.FromHours(8);
        }
    }
}
