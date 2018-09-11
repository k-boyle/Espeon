using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.ModuleBases;
using Umbreon.Commands.TypeReaders;
using Umbreon.Core.Entities.User;
using Umbreon.Extensions;
using Umbreon.Helpers;
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
        [Summary("See how many rare candies someone has")]
        [Usage("candies Umbreon")]
        public Task CandyCount(
            [Name("User")]
            [Summary("The user you want to see the candies for. Leave blank for yourself")]
            [Remainder] SocketGuildUser user = null)
            => SendMessageAsync($"{(user is null ? "You have" : $"{user.GetDisplayName()} has")} {_candy.GetCandies(user?.Id ?? Context.User.Id)}{EmotesHelper.Emotes["rarecandy"]} rare candies");

        [Command("claim")]
        [Name("Claim Candies")]
        [Summary("Claim your daily reward of rare candies")]
        [Usage("claim")]
        public async Task ClaimCandies()
        {
            if (!_candy.CanClaim(Context.User.Id))
            {
                var remaining = _database.GetObject<UserObject>("users", Context.User.Id).LastClaimed.AddHours(8) - DateTime.UtcNow;

                await SendMessageAsync($"You have already claimed your candies{EmotesHelper.Emotes["rarecandy"]} today. Please wait {remaining.Humanize()}");
                return;
            }

            var amount = _random.Next(1, 11);
            _candy.UpdateCandies(Context.User.Id, true, amount);
            await SendMessageAsync($"You have received {amount}{EmotesHelper.Emotes["rarecandy"]} rare cand{(amount > 1 ? "ies" : "y")}!");
        }

        [Command("treat")]
        [Name("Send Treats")]
        [Summary("Give a user some rare candies")]
        [Usage("treat 100 Umbreon")]
        [RequireOwner]
        public async Task GiveCandies([OverrideTypeReader(typeof(CandyTypeReader))] int amount, [Remainder] SocketGuildUser user = null)
        {
            user = user ?? Context.User;

            _candy.UpdateCandies(user.Id, false, amount);
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
        [Usage("gift Umbreon 20")]
        public async Task GiftCandies(
            [Name("User")]
            [Summary("The user you want to gift")] SocketGuildUser user,
            [Name("Amount")]
            [Summary("The amount you want to gift")]
            [OverrideTypeReader(typeof(CandyTypeReader))] int amount)
        {
            if (amount > _candy.GetCandies(Context.User.Id))
            {
                await SendMessageAsync("You don't have enough rare candies");
                return;
            }

            _candy.UpdateCandies(Context.User.Id, false, (int)-amount);
            _candy.UpdateCandies(user.Id, false, (int)amount);
            await SendMessageAsync("Your gift basket has been made and sent");
        }
    }
}
