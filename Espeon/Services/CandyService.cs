using Espeon.Attributes;
using Espeon.Commands;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class CandyService : BaseService
    {
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        public async Task UpdateCandiesAsync(EspeonContext context, ulong id, int amount)
        {
            var user = await context.Database.Users.FindAsync(id);
            user.CandyAmount += amount;

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;
            
            await context.Database.SaveChangesAsync();
        }

        public async Task ClaimCandiesAsync(EspeonContext context, ulong id)
        {
            var user = await context.Database.Users.FindAsync(id);

            user.CandyAmount += Random.Next(1, 21);

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;

            user.LastClaimedCandies = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
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
