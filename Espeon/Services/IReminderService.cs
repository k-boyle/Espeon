using Espeon.Commands;
using Espeon.Databases.UserStore;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Reminder = Espeon.Databases.Reminder;

namespace Espeon.Services
{
    public interface IReminderService
    {
        Task LoadRemindersAsync(UserStore ctx);
        Task<Reminder> CreateReminderAsync(EspeonContext context, string content, TimeSpan when);
        Task CancelReminderAsync(EspeonContext context, Reminder reminder);
        Task<ImmutableArray<Reminder>> GetRemindersAsync(EspeonContext context);
    }
}
