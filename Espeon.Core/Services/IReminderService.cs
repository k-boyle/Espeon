using Espeon.Core.Commands;
using Espeon.Core.Databases.UserStore;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Reminder = Espeon.Core.Databases.Reminder;

namespace Espeon.Core.Services {
	public interface IReminderService {
		Task LoadRemindersAsync(UserStore ctx);
		Task<Reminder> CreateReminderAsync(EspeonContext context, string content, TimeSpan when);
		Task CancelReminderAsync(EspeonContext context, Reminder reminder);
		Task<ImmutableArray<Reminder>> GetRemindersAsync(EspeonContext context);
	}
}