using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<IEnumerable<UserReminder>> GetRemindersAsync() {
            this._logger.LogDebug("Fetching all reminders");
            return await UserReminders.ToListAsync();
        }
        
        public async Task<IEnumerable<UserReminder>> GetRemindersAsync(Predicate<UserReminder> predicate) {
            this._logger.LogDebug("Fetching specific reminders");
            return (await UserReminders.ToListAsync()).Where(reminder => predicate(reminder));
        }
    }
}