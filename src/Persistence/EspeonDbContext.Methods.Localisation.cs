using Disqord;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<UserLocalisation> GetLocalisationAsync(Snowflake memberId, Snowflake guildId) {
            this._logger.Debug("Loading localisation for user {user}", memberId);
            return await UserLocalisations.FindAsync(guildId.RawValue, memberId.RawValue)
                ?? await NewUserLocalisationAsync(memberId, guildId);
        }
        
        private async Task<UserLocalisation> NewUserLocalisationAsync(Snowflake memberId, Snowflake guildId) {
            this._logger.Debug("Creating new user localisation for {user}", memberId);
            var localisation = new UserLocalisation(guildId, memberId);
            await PersistAsync(localisation);
            return localisation;
        }
    }
}