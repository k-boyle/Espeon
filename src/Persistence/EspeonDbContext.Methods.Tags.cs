using Disqord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<GuildTag> GetTagAsync(IGuild guild, string name) {
            return await GetTagAsync(guild,tag => tag.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
        
        public async Task<GuildTag> GetTagAsync(IGuild guild, Predicate<GuildTag> predicate) {
            var tags = await GuildTags.Include(tags => tags.Values)
                .FirstOrDefaultAsync(tags => tags.GuildId == guild.Id.RawValue);
            return tags.Values.FirstOrDefault(tag => predicate(tag));
        }
        
        public async Task PersistTagAsync(GuildTag tag) {
            var tags = await GuildTags.FindAsync(tag.GuildId);
            tags.Values.Add(tag);
            await UpdateAsync(tags);
        }
    }
}