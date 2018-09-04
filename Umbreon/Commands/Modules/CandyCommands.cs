using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Umbreon.Attributes;
using Umbreon.Commands.ModuleBases;
using Umbreon.Core.Entities.User;
using Umbreon.Extensions;
using Umbreon.Services;
using Colour = Discord.Color;

namespace Umbreon.Commands.Modules
{
    [Name("Candy Commands")]
    [Summary("Commands to interact with your rare candies")]
    public class CandyCommands : UmbreonBase
    {
        private readonly CandyService _candy;
        private readonly DatabaseService _database;
        private readonly Random _random;

        public CandyCommands(CandyService candy, DatabaseService database, Random random)
        {
            _candy = candy;
            _database = database;
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

            var amount = _random.Next(1, 11);
            _candy.UpdateCandies(Context.User.Id, true, amount);
            await SendMessageAsync($"You have received {amount}🍬 rare cand{(amount > 1 ? "ies" : "y")}!");
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
            await SendMessageAsync($"{user.GetDisplayName()} has been sent {amount}🍬 rare cand{(amount > 1 ? "ies" : "y")}");
        }

        [Command("leaderboard")]
        [Name("Candy Leaderboard")]
        [Alias("lb")]
        [Summary("View the top candy holders")]
        [Usage("leaderboard")]
        public async Task Leaderboard()
        {
            var count = 1;
            var users = DatabaseService.GrabAllData<UserObject>("users").OrderByDescending(x => x.RareCandies).Select(x => $"{count++} - {(Context.Guild.GetUser(x.Id)?.GetDisplayName() ?? Context.Client.GetUser(x.Id)?.Username) ?? $"{x.Id}"} : {x.RareCandies}").ToArray();
            await SendMessageAsync(string.Empty, embed: new EmbedBuilder
            {
                Title = "Leaderboard",
                Description = $"{string.Join('\n', users, 0, users.Length < 10 ? users.Length : 10)}",
                Color = Colour.Blue
            }.Build());
        }
    }
}
