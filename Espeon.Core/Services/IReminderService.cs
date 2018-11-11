using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Espeon.Core.Commands;
using Espeon.Core.Entities;

namespace Espeon.Core.Services
{
    public interface IReminderService
    {
        Task<BaseReminder> CreateReminderAsync(IEspeonContext context, string content, TimeSpan when);
        Task CancelReminderAsync (IEspeonContext context, BaseReminder removable);
        Task<IReadOnlyCollection<BaseReminder>> GetRemindersAsync(IEspeonContext context);
    }
}
