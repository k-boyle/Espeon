using Disqord;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonDbContext {
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
    }
}