using Disqord;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
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
                
                case Tag tag:
                    Tags.Update(tag);
                    break;
            }
            
            await SaveChangesAsync();
        }
    }
}