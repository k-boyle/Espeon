using Espeon.Core.Commands;
using Espeon.Core.Entities;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Espeon.Core.Services
{
    public interface IReminderService
    {
        Task<BaseReminder> CreateReminderAsync(IEspeonContext context, string content, TimeSpan when);
        Task CancelReminderAsync (IEspeonContext context, BaseReminder removable);
        Task<ImmutableArray<BaseReminder>> GetRemindersAsync(IEspeonContext context);
    }
}
