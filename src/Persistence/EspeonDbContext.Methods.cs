using Disqord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<GuildPrefixes> GetPrefixesAsync(IGuild guild) {
            this._logger.Debug("Loading prefixes for guild {Guild}", guild.Name);
            return await GuildPrefixes.SingleOrDefaultAsync(prefix => prefix.GuildId == guild.Id);
        }

        public async Task PersistGuildAsync(IGuild guild) {
            this._logger.Debug("Persisting {Guild}", guild.Name);
            if (await GuildPrefixes.FindAsync(guild.Id.RawValue) != null) {
                return;
            }
            
            await GuildPrefixes.AddAsync(new GuildPrefixes(guild.Id));
            await SaveChangesAsync();
        }
        
        public async Task RemoveGuildAsync(IGuild guild) {
            this._logger.Debug("Removing {Guild}", guild.Name);
            var prefixes = await GuildPrefixes.FindAsync(guild.Id.RawValue);
            GuildPrefixes.Remove(prefixes);
            await SaveChangesAsync();
        }
        
        public async Task<UserLocalisation> GetLocalisationAsync(IGuild guild, IUser user) {
            this._logger.Debug("Loading localisation for user {User}", user.Id);
            return await UserLocalisations.FindAsync(guild.Id.RawValue, user.Id.RawValue)
                 ?? await NewUserLocalisationAsync(guild, user);
        }
        
        private async Task<UserLocalisation> NewUserLocalisationAsync(IGuild guild, IUser user) {
            this._logger.Debug("Creating new user localisation for {User}", user.Id);
            var localisation = new UserLocalisation(guild.Id, user.Id);
            await UserLocalisations.AddAsync(localisation);
            await SaveChangesAsync();
            return localisation;
        }
        
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
        
        public async Task UpdateAsync<T>(T newData) where T : class {
            this._logger.Debug("Updating {@Data}", newData);
            switch (newData) {
                case GuildPrefixes prefixes:
                    GuildPrefixes.Update(prefixes);
                    break;
                
                case UserLocalisation localisation:
                    UserLocalisations.Update(localisation);
                    break;
                
                case UserReminder reminder:
                    UserReminders.Update(reminder);
                    break;
            }
            
            await SaveChangesAsync();
        }
    }
}