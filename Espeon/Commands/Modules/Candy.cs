﻿using Discord;
using Espeon.Commands.Checks;
using Espeon.Commands.TypeParsers;
using Espeon.Databases.Entities;
using Espeon.Extensions;
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

            await SendOkAsync(0, user.GetDisplayName(), amount, RareCandy, amount == 1 ? "y" : "ies");
        }

        [Command("Claim")]
        [Name("Claim Candies")]
        public async Task ClaimCandiesAsync()
        {
            var (IsSuccess, Amount, Cooldown) = await CandyService.TryClaimCandiesAsync(Context, Context.User);

            if (IsSuccess)
            {
                await SendOkAsync(0, Amount, RareCandy, Amount == 1 ? "y" : "ies");
                return;
            }

            await SendNotOkAsync(1, Cooldown.Humanize(2));
        }

        [Command("House")]
        [Name("View House")]
        public async Task ViewHouseAsync()
        {
            var amount = await CandyService.GetCandiesAsync(Context, Context.Client.CurrentUser);

            await SendOkAsync(0, amount, RareCandy, amount == 1 ? "y" : "ies");
        }

        [Command("Treat")]
        [Name("Treat")]
        [RequireOwner]
        public async Task TreatUserAsync(int amount, [Remainder] IGuildUser user = null)
        {
            user ??= Context.User;

            await CandyService.UpdateCandiesAsync(Context, user.Id, amount);

            await SendOkAsync(0, user.GetDisplayName(), amount, RareCandy, amount == 1 ? "y" : "ies");
        }

        [Command("Leaderboard")]
        [Name("Candy Leaderboard")]
        public async Task ViewLeaderboardAsync()
        {
            var users = await Context.UserStore.GetAllUsersAsync();
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

            await SendOkAsync(0, sb);
        }

        [Command("Gift")]
        [Name("Gift Candies")]
        public async Task GiftCandiesAsync(IGuildUser user, 
            [OverrideTypeParser(typeof(CandyTypeParser))]
            [RequireRange(0)]
            int amount)
        {
            await CandyService.TransferCandiesAsync(Context, Context.User, user, amount);

            await SendOkAsync(0);
        }

        [Command("Steal")]
        [Name("Try Steal")]
        public async Task TryStealAsync(
            [OverrideTypeParser(typeof(CandyTypeParser))]
            [RequireRange(0)]
            int amount)
        {
            var espeon = await Context.UserStore.GetOrCreateUserAsync(Context.Client.CurrentUser);
            var espeonCandies = espeon.CandyAmount;

            if (espeonCandies < 1000)
            {
                await SendNotOkAsync(0 , RareCandy);
                return;
            }

            var chance = (float)amount / espeonCandies * 0.2f;

            if(chance < Random.NextDouble())
            {
                await CandyService.UpdateCandiesAsync(Context, Context.User.Id, -amount);

                await SendNotOkAsync(1);
                return;
            }

            await CandyService.UpdateCandiesAsync(Context, Context.User.Id, espeonCandies);

            espeon.CandyAmount = 0;
            await Context.UserStore.SaveChangesAsync();

            await SendOkAsync(2);
        }
    }
}
