using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Umbreon.Attributes;
using Umbreon.Commands.ModuleBases;
using Umbreon.Extensions;
using Umbreon.Services;

namespace Umbreon.Commands.Modules
{
    [Name("Candy Commands")]
    [Summary("Commands to interact with your rare candies")]
    public class CandyCommands : UmbreonBase
    {
        private readonly CandyService _candy;
        private readonly Random _random;

        public CandyCommands(CandyService candy, Random random)
        {
            _candy = candy;
            _random = random;
        }

        [Command("candies")]
        [Name("Candies")]
        [Summary("See how many rare candies you have")]
        [Usage("candies")]
        public Task CandyCount()
            => SendMessageAsync($"You have {_candy.GetCandies(Context.User.Id)}🍬 rare candies");

        [Command("claim")]
        [Name("Claim Candies")]
        [Summary("Claim your daily reward of rare candies")]
        [Usage("claim")]
        public async Task ClaimCandies()
        {
            if (!_candy.CanClaim(Context.User.Id))
            {
                await SendMessageAsync("You have already claimed your candies 🍬 today");
                return;
            }

            var amount = _random.Next(10);
            _candy.UpdateCandies(Context.User.Id, true, amount);
            await SendMessageAsync($"You have received {amount}🍬 rare candies!");
        }

        [Command("treat")]
        [Name("Send Treats")]
        [Summary("Give a user some rare candies")]
        [Usage("treat Umbreon 100")]
        [RequireOwner]
        public async Task GiveCandies(int amount, [Remainder] SocketGuildUser user = null)
        {
            user = user ?? Context.User;

            _candy.UpdateCandies(user.Id, false, amount);
            await SendMessageAsync($"{user.GetDisplayName()} has been sent {amount}🍬 rare candies");
        }
    }
}
