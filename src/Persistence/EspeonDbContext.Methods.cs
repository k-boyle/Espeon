using Disqord;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task PersistGuildAsync(IGuild guild) {
            this._logger.Debug("Persisting {Guild}", guild.Name);
            var guildId = guild.Id.RawValue;
            if (await GuildPrefixes.FindAsync(guildId) is null) {
                await GuildPrefixes.AddAsync(new GuildPrefixes(guild.Id));
            }
            
            if (await GuildTags.FindAsync(guildId) is null) {
                await GuildTags.AddAsync(new GuildTags(guildId));
            }
            
            await SaveChangesAsync();
        }
        
        public async Task RemoveGuildAsync(IGuild guild) {
            this._logger.Debug("Removing {Guild}", guild.Name);
            var guildId = guild.Id.RawValue;
            var prefixes = await GuildPrefixes.FindAsync(guildId);
            GuildPrefixes.Remove(prefixes);
            var tags = await GuildTags.FindAsync(guildId);
            GuildTags.Remove(tags);
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
                
                case GuildTags tags:
                    GuildTags.Update(tags);
                    break;
            }
            
            await SaveChangesAsync();
        }
    }
}