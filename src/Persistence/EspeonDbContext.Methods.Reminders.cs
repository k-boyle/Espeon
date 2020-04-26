using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<IEnumerable<UserReminder>> GetRemindersAsync() {
            this._logger.Debug("Fetching all reminders");
            return await UserReminders.ToListAsync();
        }
        
        public async Task<IEnumerable<UserReminder>> GetRemindersAsync(Predicate<UserReminder> predicate) {
            this._logger.Debug("Fetching specific reminders");
            return (await UserReminders.ToListAsync()).Where(reminder => predicate(reminder));
        }
        
        public async Task PersistReminderAsync(UserReminder reminder) {
            this._logger.Debug("Persisting reminder for {User}", reminder.UserId);
            await UserReminders.AddAsync(reminder);
            await SaveChangesAsync();
        }
        
        public async Task RemoveReminderAync(UserReminder reminder) {
            this._logger.Debug("Removing reminder {Id}", reminder.Id);
            UserReminders.Remove(reminder);
            await SaveChangesAsync();
        }
    }
}