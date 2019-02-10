using Discord;
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
            var bot = context.Client.CurrentUser;

            if(amount < 0 && id != bot.Id)
            {
                var espeon = await context.Database.GetOrCreateUserAsync(bot);

                espeon.CandyAmount += Math.Abs(amount);
            }

            var user = await context.Database.GetOrCreateUserAsync(context.User);
            user.CandyAmount += amount;

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;
            
            await context.Database.SaveChangesAsync();
        }        

        public async Task TransferCandiesAsync(EspeonContext context, IUser sender, IUser receiver, int amount)
        {
            var foundSender = await context.Database.GetOrCreateUserAsync(sender);
            var foundReceiver = await context.Database.GetOrCreateUserAsync(receiver);

            foundSender.CandyAmount -= amount;
            foundReceiver.CandyAmount += amount;

            if (foundReceiver.CandyAmount > foundReceiver.HighestCandies)
                foundReceiver.HighestCandies = foundReceiver.CandyAmount;

            await context.Database.SaveChangesAsync();
        }

        public async Task<int> GetCandiesAsync(EspeonContext context, IUser user)
        {
            var foundUser = await context.Database.GetOrCreateUserAsync(user);
            return foundUser.CandyAmount;
        }

        public async Task<(bool IsSuccess, int Amount, TimeSpan Cooldown)> TryClaimCandiesAsync(EspeonContext context, IUser toClaim)
        {
            var user = await context.Database.GetOrCreateUserAsync(toClaim);
            var difference = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(user.LastClaimedCandies);

            if (difference < TimeSpan.FromHours(8))
            {
                return (false, 0, TimeSpan.FromHours(8) - difference);
            }

            var amount = Random.Next(1, 21);
            user.CandyAmount += amount;

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;

            user.LastClaimedCandies = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await context.Database.SaveChangesAsync();

            return (true, amount, TimeSpan.FromHours(8));
        }
    }
}
