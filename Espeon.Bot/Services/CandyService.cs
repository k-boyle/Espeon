using Casino.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Databases.UserStore;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Services
{
    public class CandyService : BaseService<InitialiseArgs>, ICandyService
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly Config _config;
        [Inject] private readonly IEventsService _events;
        [Inject] private readonly Random _random;

        private TimeSpan Cooldown => TimeSpan.FromHours(_config.ClaimCooldown);

        public CandyService(IServiceProvider services) : base(services)
        {
            _client.MessageReceived += msg => _events.RegisterEvent(async () =>
            {
                if (msg.Channel is IDMChannel)
                    return;

                if (_random.NextDouble() >= _config.RandomCandyFrequency)
                    return;

                using var userStore = services.GetService<UserStore>();

                var user = await userStore.GetOrCreateUserAsync(msg.Author);
                user.CandyAmount += _config.RandomCandyAmount;

                if (user.HighestCandies > user.CandyAmount)
                    user.HighestCandies = user.CandyAmount;

                userStore.Update(user);

                await userStore.SaveChangesAsync();
            });
        }

        async Task ICandyService.UpdateCandiesAsync(EspeonContext context, UserStore store, IUser user, int amount)
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

         async Task ICandyService.TransferCandiesAsync(EspeonContext context, IUser sender, IUser receiver, int amount)
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

        async Task<int> ICandyService.GetCandiesAsync(EspeonContext context, IUser user)
        {
            var foundUser = await context.UserStore.GetOrCreateUserAsync(user);
            return foundUser.CandyAmount;
        }

        async Task<(bool IsSuccess, int Amount, TimeSpan Cooldown)> ICandyService.TryClaimCandiesAsync(EspeonContext context, IUser toClaim)
        {
            var user = await context.UserStore.GetOrCreateUserAsync(toClaim);
            var difference = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(user.LastClaimedCandies);

            if (difference < Cooldown)
            {
                return (false, 0, Cooldown - difference);
            }

            var amount = _random.Next(_config.ClaimMin, _config.ClaimMax + 1);
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
