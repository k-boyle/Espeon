using Disqord;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task PersistGuildAsync(IGuild guild) {
            this._logger.Debug("Persisting {guild}", guild.Name);
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
            this._logger.Debug("Removing {guild}", guild.Name);
            var guildId = guild.Id.RawValue;
            var prefixes = await GuildPrefixes.FindAsync(guildId);
            GuildPrefixes.Remove(prefixes);
            var tags = await GuildTags.FindAsync(guildId);
            GuildTags.Remove(tags);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync<T>(T newData) where T : class {
            this._logger.Debug("Updating {@data}", newData);
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

                case GuildTags tags:
                    GuildTags.Update(tags);
                    break;

                case Tag tag:
                    Tags.Update(tag);
                    break;
            }
            
            await SaveChangesAsync();
        }
        
        public async Task PersistAsync<T>(T data) where T : class {
            this._logger.Debug("Persisting {@data}", data);
            switch (data) {
                case GuildPrefixes prefixes:
                    await GuildPrefixes.AddAsync(prefixes);
                    break;
                
                case UserLocalisation localisation:
                    await UserLocalisations.AddAsync(localisation);
                    break;
                
                case UserReminder reminder:
                    await UserReminders.AddAsync(reminder);
                    break;

                case GuildTags tags:
                    await GuildTags.AddAsync(tags);
                    break;
                
                case GuildTag tag: {
                    var tags = await GuildTags.FindAsync(tag.GuildId);
                    tags.Values.Add(tag);
                    GuildTags.Update(tags);
                    break;
                }
                
                case Tag tag:
                    await Tags.AddAsync(tag);
                    break;
            }

            await SaveChangesAsync();
        }
        
        public async Task RemoveAsync<T>(T data) where T : class {
            this._logger.Debug("Removing {@data}", data);
            switch (data) {
                case GuildPrefixes prefixes:
                    GuildPrefixes.Remove(prefixes);
                    break;
                
                case UserLocalisation localisation:
                    UserLocalisations.Remove(localisation);
                    break;
                
                case UserReminder reminder:
                    UserReminders.Remove(reminder);
                    break;

                case GuildTags tags:
                    GuildTags.Remove(tags);
                    break;
                
                case GuildTag tag: {
                    var tags = await GuildTags.FindAsync(tag.GuildId);
                    tags.Values.Remove(tag);
                    GuildTags.Update(tags);
                    break;
                }
                
                case Tag tag:
                    Tags.Remove(tag);
                    break;
            }

            await SaveChangesAsync();
        }
    }
}