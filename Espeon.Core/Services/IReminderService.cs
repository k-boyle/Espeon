using Disqord;
using Espeon.Core.Database;
using Espeon.Core.Database.UserStore;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Espeon.Core.Services {
	public interface IReminderService {
		Task LoadRemindersAsync(UserStore ctx);
		Task<Reminder> CreateReminderAsync(UserStore userStore, ulong guildId, IUserMessage message,
			string content, TimeSpan when);
		Task CancelReminderAsync(UserStore userStore, Reminder reminder);
		Task<ImmutableArray<Reminder>> GetRemindersAsync(UserStore userStore, IUser user);
	}
}