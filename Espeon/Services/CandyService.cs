using Espeon.Attributes;
using Espeon.Commands;
using Espeon.Database;
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

        public async Task UpdateCandiesAsync(EspeonContext context, ulong id, int amount)
        {
            var user = await context.Database.Users.FindAsync(id);
            user.CandyAmount += amount;

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;

            context.Database.Users.Update(user);
            await context.Database.SaveChangesAsync();
        }

        public async Task ClaimCandiesAsync(EspeonContext context, ulong id)
        {
            var user = await context.Database.Users.FindAsync(id);

            user.CandyAmount += Random.Next(1, 21);

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;

            user.LastClaimedCandies = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            context.Database.Users.Update(user);

            await context.Database.SaveChangesAsync();
        }

        public async Task<int> GetCandiesAsync(EspeonContext context, ulong id)
        {
            var user = await context.Database.Users.FindAsync(id);
            return user.CandyAmount;
        }

        public async Task<bool> CanClaimCandiesAsync(EspeonContext context, ulong id)
        {
            var user = await context.Database.Users.FindAsync(id);
            return DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(user.LastClaimedCandies) >
                   TimeSpan.FromHours(8);
        }
    }
}
