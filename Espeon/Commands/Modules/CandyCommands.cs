using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Espeon.Attributes;
using Espeon.Commands.ModuleBases;
using Espeon.Commands.TypeReaders;
using Espeon.Core.Entities.User;
using Espeon.Extensions;
using Espeon.Helpers;
using Espeon.Services;
using Humanizer;
using System;
using System.Linq;
using System.Threading.Tasks;
using Colour = Discord.Color;

namespace Espeon.Commands.Modules
{
    [Name("Candy Commands")]
    [Summary("Commands to interact with your rare candies")]
    public class CandyCommands : EspeonBase
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
        [Summary("See how many rare candies someone has")]
        [Usage("candies Espeon")]
        public async Task CandyCount(
            [Name("User")]
            [Summary("The user you want to see the candies for. Leave blank for yourself")]
            [Remainder] SocketGuildUser user = null)
            => await SendMessageAsync($"{(user is null ? "You have" : $"{user.GetDisplayName()} has")} {await _candy.GetCandiesAsync(user?.Id ?? Context.User.Id)}{EmotesHelper.Emotes["rarecandy"]} rare candies");

        [Command("claim")]
        [Alias("c")]
        [Name("Claim Candies")]
        [Summary("Claim your daily reward of rare candies")]
        [Usage("claim")]
        public async Task ClaimCandies()
        {
            if (!await _candy.CanClaimAsync(Context.User.Id))
            {
                var remaining = (await _database.GetObjectAsync<UserObject>("users", Context.User.Id)).LastClaimed.ToUniversalTime().AddHours(8) - DateTime.UtcNow;

                await SendMessageAsync($"You recently claimed your candies{EmotesHelper.Emotes["rarecandy"]}, wait {remaining.Humanize(2)}");
                return;
            }

            var amount = _random.Next(1, 11);
            await _candy.UpdateCandiesAsync(Context.User.Id, true, amount);
            await SendMessageAsync($"You have received {amount}{EmotesHelper.Emotes["rarecandy"]} rare cand{(amount > 1 ? "ies" : "y")}!");
        }

        [Command("house")]
        [Name("House Balance")]
        [Summary("View how many candies the house has won vs suckers who gambled")]
        [Usage("house")]
        public async Task SeeHouse()
            => await SendMessageAsync($"The house currently has {await _candy.GetCandiesAsync(Context.Guild.CurrentUser.Id)}{EmotesHelper.Emotes["rarecandy"]} rare candies");

        [Command("treat")]
        [Name("Send Treats")]
        [Summary("Give a user some rare candies")]
        [Usage("treat 100 Espeon")]
        [RequireOwner]
        public async Task GiveCandies(int amount, [Remainder] SocketGuildUser user = null)
        {
            user = user ?? Context.User;

            await _candy.UpdateCandiesAsync(user.Id, false, amount, isGift: true);
            await SendMessageAsync($"{user.GetDisplayName()} has been sent {amount}{EmotesHelper.Emotes["rarecandy"]} rare cand{(amount > 1 ? "ies" : "y")}");
        }

        [Command("leaderboard")]
        [Name("Candy Leaderboard")]
        [Alias("lb")]
        [Summary("View the top candy holders")]
        [Usage("leaderboard")]
        public async Task Leaderboard()
        {
            var count = 1;
            var users = DatabaseService.GrabAllData<UserObject>("users").OrderByDescending(x => x.RareCandies).Select(x => $"{count++} - {Context.Client.GetUser(x.Id)?.Username ?? $"<@{x.Id}>"} : {x.RareCandies}").ToArray();
            await SendMessageAsync(string.Empty, embed: new EmbedBuilder
            {
                Title = "Leaderboard",
                Description = $"{string.Join('\n', users, 0, users.Length < 10 ? users.Length : 10)}",
                Color = Colour.Blue
            }.Build());
        }

        [Command("gift")]
        [Name("Gift Candies")]
        [Summary("Send candies to someone")]
        [Usage("gift Espeon 20")]
        public async Task GiftCandies(
            [Name("User")]
            [Summary("The user you want to gift")] SocketGuildUser user,
            [Name("Amount")]
            [Summary("The amount you want to gift")]
            [OverrideTypeReader(typeof(CandyTypeReader))] int amount)
        {
            if (amount > await _candy.GetCandiesAsync(Context.User.Id))
            {
                await SendMessageAsync("You don't have enough rare candies");
                return;
            }

            await _candy.UpdateCandiesAsync(Context.User.Id, false, -amount);
            await _candy.UpdateCandiesAsync(user.Id, false, amount);
            await SendMessageAsync("Your gift basket has been made and sent");
        }

        [Command("steal")]
        [Name("Steal Candies")]
        [Summary("Attempt to steal candies from the house. You have a (bet/house) * 0.2 % chance of succeeding")]
        [Usage("steal 100")]
        public async Task TrySteal(
            [Name("Amount")]
            [Summary("The amount you want to bet")]
            [OverrideTypeReader(typeof(CandyTypeReader))] int amount)
        {
            if (await _candy.TryStealAsync(Context.User.Id, amount))
            {
                await SendMessageAsync("Well done! You win!");
                return;
            }

            await SendMessageAsync("Thanks for the candies, chump");
        }

        [Command("history")]
        [Name("Transaction History")]
        [Summary("View the last 5 transactions")]
        [Usage("history")]
        public Task ViewHistory()
            => SendMessageAsync(string.IsNullOrEmpty(_candy.GetLogs()) ? "None" : _candy.GetLogs());
    }
}
