using Disqord;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
        public async Task<UserLocalisation> GetLocalisationAsync(IGuild guild, IUser user) {
            this._logger.Debug("Loading localisation for user {user}", user.Id);
            return await UserLocalisations.FindAsync(guild.Id.RawValue, user.Id.RawValue)
                ?? await NewUserLocalisationAsync(guild, user);
        }
        
        private async Task<UserLocalisation> NewUserLocalisationAsync(IGuild guild, IUser user) {
            this._logger.Debug("Creating new user localisation for {user}", user.Id);
            var localisation = new UserLocalisation(guild.Id, user.Id);
            await PersistAsync(localisation);
            return localisation;
        }
    }
}