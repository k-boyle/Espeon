using Disqord;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<UserLocalisation> GetLocalisationAsync(CachedMember member) {
            this._logger.Debug("Loading localisation for user {user}", member.Id);
            return await UserLocalisations.FindAsync(member.Guild.Id.RawValue, member.Id.RawValue)
                ?? await NewUserLocalisationAsync(member);
        }
        
        private async Task<UserLocalisation> NewUserLocalisationAsync(CachedMember member) {
            this._logger.Debug("Creating new user localisation for {user}", member.Id);
            var localisation = new UserLocalisation(member.Guild.Id, member.Id);
            await PersistAsync(localisation);
            return localisation;
        }
    }
}