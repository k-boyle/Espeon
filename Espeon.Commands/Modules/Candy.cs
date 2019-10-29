using Disqord;
using Espeon.Core;
using Espeon.Core.Database;
using Espeon.Core.Services;
using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Commands {
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
	public class Candy : EspeonModuleBase {
		public ICandyService CandyService { get; set; }
		public IEmoteService Emotes { get; set; }
		public Random Random { get; set; }

		public CachedGuildEmoji RareCandy => Emotes["RareCandy"];

		[Command("Candies")]
		[Name("View Candies")]
		[Description("See how many rare candies you, or the specified user has")]
		public async Task ViewCandiesAsync([Remainder] IMember user = null) {
			user ??= Member;

			int amount = await CandyService.GetCandiesAsync(Context.UserStore, user);

			await SendOkAsync(0, user.DisplayName, amount, RareCandy, amount == 1 ? "y" : "ies");
		}

		[Command("Claim")]
		[Name("Claim Candies")]
		[Description("Claim your free candies")]
		public async Task ClaimCandiesAsync() {
			(bool isSuccess, int amount, TimeSpan cooldown) = await CandyService.TryClaimCandiesAsync(Context.UserStore, Member);

			if (isSuccess) {
				await SendOkAsync(0, amount, RareCandy, amount == 1 ? "y" : "ies");
				return;
			}

			await SendNotOkAsync(1, cooldown.Humanize(2));
		}

		[Command("House")]
		[Name("View House")]
		[Description("See how many candies the bot has")]
		public async Task ViewHouseAsync() {
			int amount = await CandyService.GetCandiesAsync(Context.UserStore, Client.CurrentUser);

			await SendOkAsync(0, amount, RareCandy, amount == 1 ? "y" : "ies");
		}

		[Command("Treat")]
		[Name("Treat")]
		[RequireOwner]
		[Description("Generate free candies for the specified user")]
		public Task TreatUserAsync(int amount, [Remainder] IMember user = null) {
			user??=Member;

			return Task.WhenAll(CandyService.UpdateCandiesAsync(Context.UserStore, Client.CurrentUser, user, amount),
				SendOkAsync(0, user.DisplayName, amount, RareCandy, amount == 1 ? "y" : "ies"));
		}

		[Command("Leaderboard")]
		[Name("Candy Leaderboard")]
		[Description("See the current top candy holders")]
		public async Task ViewLeaderboardAsync() {
			IReadOnlyCollection<User> users = await Context.UserStore.GetAllUsersAsync();
			User[] ordered = users.OrderByDescending(x => x.CandyAmount).ToArray();

			var foundUsers = new List<(IUser, User)>();

			foreach (User user in ordered) {
				if (foundUsers.Count == 10) {
					break;
				}

				IUser found = await Guild.GetOrFetchMemberAsync(user.Id) as IUser ?? await Client.GetOrFetchUserAsync(user.Id);

				if (found is null) {
					continue;
				}

				foundUsers.Add((found, user));
			}

			var sb = new StringBuilder();
			var i = 1;

			foreach ((IUser found, User user) in foundUsers) {
				if (found is IMember guildMember) {
					sb.Append(i++).Append(": ").Append(guildMember.DisplayName).Append(" - ")
						.Append(user.CandyAmount).AppendLine();
				} else {
					sb.Append(i++).Append(": ").Append(found.Name).Append(" - ").Append(user.CandyAmount)
						.AppendLine();
				}
			}

			await SendOkAsync(0, sb);
		}

		[Command("Gift")]
		[Name("Gift Candies")]
		[Description("Gift candies to another user")]
		public Task GiftCandiesAsync(IMember user,
			[OverrideTypeParser(typeof(CandyTypeParser))] [RequireRange(0)] int amount) {
			return user.Id == Member.Id
				? SendNotOkAsync(0)
				: Task.WhenAll(CandyService.TransferCandiesAsync(Context.UserStore, Member, user, amount), SendOkAsync(1));
		}

		[Command("Steal")]
		[Name("Try Steal")]
		[Description("Attemps to steal all of the bots candies")]
		public async Task TryStealAsync([OverrideTypeParser(typeof(CandyTypeParser))] [RequireRange(0)] int amount) {
			User espeon = await Context.UserStore.GetOrCreateUserAsync(Client.CurrentUser);
			int espeonCandies = espeon.CandyAmount;

			if (espeonCandies < 1000) {
				await SendNotOkAsync(0, RareCandy);
				return;
			}

			float chance = (float) amount / espeonCandies * 0.2f;

			if (chance < Random.NextDouble()) {
				await CandyService.UpdateCandiesAsync(Context.UserStore, Client.CurrentUser, Member, -amount);

				await SendNotOkAsync(1);
				return;
			}

			await CandyService.UpdateCandiesAsync(Context.UserStore, Client.CurrentUser, Member, espeonCandies);

			espeon.CandyAmount = 0;

			Context.UserStore.Update(espeon);

			await Task.WhenAll(Context.UserStore.SaveChangesAsync(), SendOkAsync(2));
		}
	}
}