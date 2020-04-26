using Disqord;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<GuildPrefixes> GetPrefixesAsync(IGuild guild) {
            this._logger.Debug("Loading prefixes for guild {Guild}", guild.Name);
            return await GuildPrefixes.SingleOrDefaultAsync(prefix => prefix.GuildId == guild.Id);
        }
    }
}