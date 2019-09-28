using Casino.Discord;
using Discord;
using Espeon.Commands;
using Espeon.Databases;
using Espeon.Services;
using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
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

    [Name("Candy Commands")]
    [Description("Do stuff with your rare candies")]
    public class Candy : EspeonModuleBase
    {
        public ICandyService CandyService { get; set; }
        public IEmoteService Emotes { get; set; }
        public Random Random { get; set; }

        public Emote RareCandy => Emotes.Collection["RareCandy"];

        [Command("Candies")]
        [Name("View Candies")]
        [Description("See how many rare candies you, or the specified user has")]
        public async Task ViewCandiesAsync([Remainder] IGuildUser user = null)
        {
            user ??= User;

            var amount = await CandyService.GetCandiesAsync(Context, user);

            await SendOkAsync(0, user.GetDisplayName(), amount, RareCandy, amount == 1 ? "y" : "ies");
        }

        [Command("Claim")]
        [Name("Claim Candies")]
        [Description("Claim your free candies")]
        public async Task ClaimCandiesAsync()
        {
            var (isSuccess, amount, cooldown) = await CandyService.TryClaimCandiesAsync(Context, User);

            if (isSuccess)
            {
                await SendOkAsync(0, amount, RareCandy, amount == 1 ? "y" : "ies");
                return;
            }

            await SendNotOkAsync(1, cooldown.Humanize(2));
        }

        [Command("House")]
        [Name("View House")]
        [Description("See how many candies the bot has")]
        public async Task ViewHouseAsync()
        {
            var amount = await CandyService.GetCandiesAsync(Context, Client.CurrentUser);

            await SendOkAsync(0, amount, RareCandy, amount == 1 ? "y" : "ies");
        }

        [Command("Treat")]
        [Name("Treat")]
        [RequireOwner]
        [Description("Generate free candies for the specified user")]
        public Task TreatUserAsync(int amount, [Remainder] IGuildUser user = null)
        {
            user ??= User;

            return Task.WhenAll(
                CandyService.UpdateCandiesAsync(Context, user, amount),
                SendOkAsync(0, user.GetDisplayName(), amount, RareCandy, amount == 1 ? "y" : "ies"));
        }

        [Command("Leaderboard")]
        [Name("Candy Leaderboard")]
        [Description("See the current top candy holders")]
        public async Task ViewLeaderboardAsync()
        {
            var users = await Context.UserStore.GetAllUsersAsync();
            var ordered = users.OrderByDescending(x => x.CandyAmount).ToArray();

            var foundUsers = new List<(IUser, User)>();

            foreach (var user in ordered)
            {
                if (foundUsers.Count == 10)
                    break;

                var found = await Guild.GetOrFetchUserAsync(user.Id)
                    ?? await Client.GetOrFetchUserAsync(user.Id);

                if (found is null)
                    continue;

                foundUsers.Add((found, user));
            }

            var sb = new StringBuilder();
            var i = 1;

            foreach (var (found, user) in foundUsers)
            {
                if (found is IGuildUser guildUser)
                    sb.Append(i++).Append(": ").Append(guildUser.GetDisplayName()).Append(" - ")
                        .Append(user.CandyAmount).AppendLine();
                else
                    sb.Append(i++).Append(": ").Append(found.Username).Append(" - ")
                        .Append(user.CandyAmount).AppendLine();
            }

            await SendOkAsync(0, sb);
        }

        [Command("Gift")]
        [Name("Gift Candies")]
        [Description("Gift candies to another user")]
        public Task GiftCandiesAsync(IGuildUser user,
            [OverrideTypeParser(typeof(CandyTypeParser))]
            [RequireRange(0)]
            int amount)
        {
            if (user.Id == User.Id)
                return SendNotOkAsync(0);

            return Task.WhenAll(
                CandyService.TransferCandiesAsync(Context, User, user, amount),
                SendOkAsync(1));
        }

        [Command("Steal")]
        [Name("Try Steal")]
        [Description("Attemps to steal all of the bots candies")]
        public async Task TryStealAsync(
            [OverrideTypeParser(typeof(CandyTypeParser))]
            [RequireRange(0)]
            int amount)
        {
            var espeon = await Context.UserStore.GetOrCreateUserAsync(Client.CurrentUser);
            var espeonCandies = espeon.CandyAmount;

            if (espeonCandies < 1000)
            {
                await SendNotOkAsync(0 , RareCandy);
                return;
            }

            var chance = (float)amount / espeonCandies * 0.2f;

            if(chance < Random.NextDouble())
            {
                await CandyService.UpdateCandiesAsync(Context, User, -amount);

                await SendNotOkAsync(1);
                return;
            }

            await CandyService.UpdateCandiesAsync(Context, User, espeonCandies);

            espeon.CandyAmount = 0;

            Context.UserStore.Update(espeon);

            await Task.WhenAll(Context.UserStore.SaveChangesAsync(), SendOkAsync(2));
        }
    }
}
