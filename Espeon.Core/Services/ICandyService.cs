using Disqord;
using Espeon.Core.Database.UserStore;
using System;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface ICandyService {
		Task UpdateCandiesAsync(UserStore store, CachedCurrentUser bot, IUser user, int amount);

		Task TransferCandiesAsync(UserStore userStore, IUser sender, IUser receiver, int amount);

		Task<int> GetCandiesAsync(UserStore userStore, IUser user);

		Task<(bool IsSuccess, int Amount, TimeSpan Cooldown)> TryClaimCandiesAsync(UserStore userStore, IUser toClaim);
	}
}