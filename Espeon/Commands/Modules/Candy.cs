using Discord;
using Espeon.Commands.TypeParsers;
using Espeon.Database.Entities;
using Espeon.Services;
using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    /*
     * Candies
     * Claim
     * House
     * Treat
     * Leaderboard
     * Gift
     * Steal
     */

    [Name("Candies")]
    public class Candy : EspeonBase
    {
        public CandyService CandyService { get; set; }
        public EmotesService Emotes { get; set; }
        public Random Random { get; set; }

        public Emote RareCandy => Emotes.Collection["RareCandy"];

        [Command("Candies")]
        [Name("View Candies")]
        public async Task ViewCandiesAsync([Remainder] IGuildUser user = null)
        {
            user ??= Context.User;

            var amount = await CandyService.GetCandiesAsync(Context, user);

            await SendOkAsync($"{user.GetDisplayName()} has {amount}{RareCandy} rare cand{(amount == 1 ? "y" : "ies")}!");
        }

        [Command("Claim")]
        [Name("Claim Candies")]
        public async Task ClaimCandiesAsync()
        {
            var (IsSuccess, Amount, Cooldown) = await CandyService.TryClaimCandiesAsync(Context, Context.User);

            if (IsSuccess)
            {
                await SendOkAsync($"You have received {Amount}{RareCandy} rare cand{(Amount == 1 ? "y" : "ies")}!");
                return;
            }

            await SendNotOkAsync($"You recently claimed your candies, please wait {Cooldown.Humanize(2)}");
        }

        [Command("House")]
        [Name("View House")]
        public async Task ViewHouseAsync()
        {
            var amount = await CandyService.GetCandiesAsync(Context, Context.Client.CurrentUser);

            await SendOkAsync($"I current have {amount}{RareCandy} rare cand{(amount == 1 ? "y" : "ies")}!");
        }

        [Command("Treat")]
        [Name("Treat")]
        public async Task TreatUserAsync(int amount, [Remainder] IGuildUser user = null)
        {
            user ??= Context.User;

            await CandyService.UpdateCandiesAsync(Context, user.Id, amount);

            await SendOkAsync($"{user.GetDisplayName()} has been treated to {amount}{RareCandy} rare cand{(amount == 1 ? "y" : "ies")}!");
        }

        [Command("Leaderboard")]
        [Name("Candy Leaderboard")]
        public async Task ViewLeaderboardAsync()
        {
            var users = await Context.Database.GetAllUsersAsync();
            var ordered = users.OrderByDescending(x => x.CandyAmount);

            var foundUsers = new List<(IUser, User)>();

            foreach (var user in ordered)
            {
                if (foundUsers.Count == 10)
                    break;

                var found = Context.Guild.GetUser(user.Id) as IUser
                    ?? await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, user.Id)
                    ?? Context.Client.GetUser(user.Id) as IUser
                    ?? await Context.Client.Rest.GetUserAsync(user.Id);

                if (found is null)
                    continue;

                foundUsers.Add((found, user));
            }

            var sb = new StringBuilder();
            var i = 1;

            foreach (var (found, user) in foundUsers)
            {
                if (found is IGuildUser guildUser)
                    sb.AppendLine($"{i++}: {guildUser.GetDisplayName()} - {user.CandyAmount}");
                else
                    sb.AppendLine($"{i++}: {found.Username} - {user.CandyAmount}");
            }

            await SendOkAsync(sb.ToString());
        }

        [Command("Gift")]
        [Name("Gift Candies")]
        public async Task GiftCandiesAsync(IGuildUser user, [OverrideTypeParser(typeof(CandyTypeParser))] int amount)
        {
            await CandyService.TransferCandiesAsync(Context, Context.User, user, amount);

            await SendOkAsync("Your gift basket has been made and sent");
        }

        [Command("Steal")]
        [Name("Try Steal")]
        public async Task TryStealAsync([OverrideTypeParser(typeof(CandyTypeParser))] int amount)
        {
            var espeon = await Context.Database.GetOrCreateUserAsync(Context.Client.CurrentUser);
            var espeonCandies = espeon.CandyAmount;

            if (espeonCandies < 1000)
            {
                await SendNotOkAsync($"Steal can only be used when I am at 1000{RareCandy} rare candies!");
                return;
            }

            var chance = (float)amount / espeonCandies * 0.2f;

            if(chance < Random.NextDouble())
            {
                await CandyService.UpdateCandiesAsync(Context, Context.User.Id, -amount);

                await SendNotOkAsync("You fail! You lose!");
                return;
            }

            await CandyService.UpdateCandiesAsync(Context, Context.User.Id, espeonCandies);

            espeon.CandyAmount = 0;
            await Context.Database.SaveChangesAsync();

            await SendOkAsync("Holy shit! You actually won!");
        }
    }
}
