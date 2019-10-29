using Casino.DependencyInjection;
using Disqord;
using Espeon.Core;
using Espeon.Core.Database;
using Espeon.Core.Database.UserStore;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class CandyService : BaseService<InitialiseArgs>, ICandyService {
		[Inject] private readonly DiscordClient _client;
		[Inject] private readonly Config _config;
		[Inject] private readonly IEventsService _events;
		[Inject] private readonly Random _random;

		private TimeSpan Cooldown => TimeSpan.FromHours(this._config.ClaimCooldown);

		public CandyService(IServiceProvider services) : base(services) {
			this._client.MessageReceived += args => this._events.RegisterEvent(async () => {
				if (args.Message.Channel is IDmChannel) {
					return;
				}

				if (this._random.NextDouble() >= this._config.RandomCandyFrequency) {
					return;
				}

				await using var userStore = services.GetService<UserStore>();

				User user = await userStore.GetOrCreateUserAsync(args.Message.Author);
				user.CandyAmount += this._config.RandomCandyAmount;

				if (user.HighestCandies > user.CandyAmount) {
					user.HighestCandies = user.CandyAmount;
				}

				userStore.Update(user);

				await userStore.SaveChangesAsync();
			});
		}

		async Task ICandyService.UpdateCandiesAsync(UserStore store, CachedCurrentUser bot, IUser user, int amount) {
			if (amount < 0 && user.Id != bot.Id) {
				User espeon = await store.GetOrCreateUserAsync(bot);

				espeon.CandyAmount += Math.Abs(amount);
				store.Update(espeon);
			}

			User dbUser = await store.GetOrCreateUserAsync(user);
			dbUser.CandyAmount += amount;

			if (dbUser.CandyAmount > dbUser.HighestCandies) {
				dbUser.HighestCandies = dbUser.CandyAmount;
			}

			store.Update(dbUser);

			await store.SaveChangesAsync();
		}

		async Task ICandyService.TransferCandiesAsync(UserStore userStore, IUser sender, IUser receiver, int amount) {
			User foundSender = await userStore.GetOrCreateUserAsync(sender);
			User foundReceiver = await userStore.GetOrCreateUserAsync(receiver);

			foundSender.CandyAmount -= amount;
			foundReceiver.CandyAmount += amount;

			if (foundReceiver.CandyAmount > foundReceiver.HighestCandies) {
				foundReceiver.HighestCandies = foundReceiver.CandyAmount;
			}

			userStore.Update(foundReceiver);
			userStore.Update(foundSender);

			await userStore.SaveChangesAsync();
		}

		async Task<int> ICandyService.GetCandiesAsync(UserStore userStore, IUser user) {
			User foundUser = await userStore.GetOrCreateUserAsync(user);
			return foundUser.CandyAmount;
		}

		async Task<(bool IsSuccess, int Amount, TimeSpan Cooldown)> ICandyService.TryClaimCandiesAsync(
			UserStore userStore, IUser toClaim) {
			User user = await userStore.GetOrCreateUserAsync(toClaim);
			TimeSpan difference =
				DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(user.LastClaimedCandies);

			if (difference < Cooldown) {
				return (false, 0, Cooldown - difference);
			}

			int amount = this._random.Next(this._config.ClaimMin, this._config.ClaimMax + 1);
			user.CandyAmount += amount;

			if (user.CandyAmount > user.HighestCandies) {
				user.HighestCandies = user.CandyAmount;
			}

			user.LastClaimedCandies = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			userStore.Update(user);

			await userStore.SaveChangesAsync();

			return (true, amount, Cooldown);
		}
	}
}