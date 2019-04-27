using Casino.Common.DependencyInjection;
using Discord;
using Espeon.Commands;
using Espeon.Databases.UserStore;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class CandyService : BaseService<InitialiseArgs>
    {
        [Inject] private readonly Config _config;
        [Inject] private Random _random;

        private TimeSpan Cooldown => TimeSpan.FromHours(_config.ClaimCooldown);

        private Random Random => _random ?? (_random = new Random());

        public CandyService(IServiceProvider services) : base(services)
        {
        }

        public Task UpdateCandiesAsync(EspeonContext context, IUser user, int amount)
            => UpdateCandiesAsync(context, context.UserStore, user, amount);

        public async Task UpdateCandiesAsync(EspeonContext context, UserStore store, IUser user, int amount)
        {
            var bot = context.Client.CurrentUser;

            if (amount < 0 && user.Id != bot.Id)
            {
                var espeon = await store.GetOrCreateUserAsync(bot);

                espeon.CandyAmount += Math.Abs(amount);
                store.Update(espeon);
            }

            var dbUser = await store.GetOrCreateUserAsync(user);
            dbUser.CandyAmount += amount;

            if (dbUser.CandyAmount > dbUser.HighestCandies)
                dbUser.HighestCandies = dbUser.CandyAmount;

            store.Update(dbUser);

            await store.SaveChangesAsync();
        }

        public async Task TransferCandiesAsync(EspeonContext context, IUser sender, IUser receiver, int amount)
        {
            var foundSender = await context.UserStore.GetOrCreateUserAsync(sender);
            var foundReceiver = await context.UserStore.GetOrCreateUserAsync(receiver);

            foundSender.CandyAmount -= amount;
            foundReceiver.CandyAmount += amount;

            if (foundReceiver.CandyAmount > foundReceiver.HighestCandies)
                foundReceiver.HighestCandies = foundReceiver.CandyAmount;

            context.UserStore.Update(foundReceiver);
            context.UserStore.Update(foundSender);

            await context.UserStore.SaveChangesAsync();
        }

        public async Task<int> GetCandiesAsync(EspeonContext context, IUser user)
        {
            var foundUser = await context.UserStore.GetOrCreateUserAsync(user);
            return foundUser.CandyAmount;
        }

        public async Task<(bool IsSuccess, int Amount, TimeSpan Cooldown)> TryClaimCandiesAsync(EspeonContext context, IUser toClaim)
        {
            var user = await context.UserStore.GetOrCreateUserAsync(toClaim);
            var difference = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(user.LastClaimedCandies);

            if (difference < Cooldown)
            {
                return (false, 0, Cooldown - difference);
            }

            var amount = Random.Next(_config.ClaimMin, _config.ClaimMax + 1);
            user.CandyAmount += amount;

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;

            user.LastClaimedCandies = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            context.UserStore.Update(user);

            await context.UserStore.SaveChangesAsync();

            return (true, amount, Cooldown);
        }
    }
}
