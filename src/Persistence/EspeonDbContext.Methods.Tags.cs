using Disqord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<GuildTag> GetTagAsync(IGuild guild, string name) {
            this._logger.Debug("Getting {Tag} for {Guild}", name, guild.Name);
            return await GetTagAsync(guild,tag => tag.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        private async Task<GuildTag> GetTagAsync(IGuild guild, Predicate<GuildTag> predicate) {
            var tags = await GuildTags.Include(tags => tags.Values)
                .FirstOrDefaultAsync(tags => tags.GuildId == guild.Id.RawValue);
            return tags.Values.FirstOrDefault(tag => predicate(tag));
        }

        public async Task<Tag> GetTagAsync<T>(string name) where T : Tag {
            this._logger.Debug("Getting {Tag}", name);
            return await Tags.FirstOrDefaultAsync(tag => tag is T && tag.Key == name);
        }

        public async Task<ICollection<T>> GetTagsAsync<T>() {
            this._logger.Debug("Getting tags");
            return await Tags.Where(tag => tag is T).Cast<T>().ToListAsync();
        }
    }
}