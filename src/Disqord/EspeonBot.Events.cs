using Disqord.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonBot {
        private async Task OnReadyAsync(ReadyEventArgs e) {
            await using var context = this.GetService<EspeonDbContext>();
            foreach (var guild in e.Client.Guilds.Values) {
                this._logger.Information("Persisting {GuildName}", guild.Name);
                await context.PersistGuildAsync(guild);
            }
            this._logger.Information("Espeon is ready!");
        }

        private async Task OnFirstReadyAsync(ReadyEventArgs e) {
            Ready -= OnFirstReadyAsync;
            await using var context = this.GetService<EspeonDbContext>();
            foreach (var service in this.GetServices<IOnReadyService>()) {
                await service.OnReadyAsync(context);
            }
        }

        private async Task OnGuildJoined(JoinedGuildEventArgs e) {
            this._logger.Information("Joined {Guild} with {Members} members", e.Guild.Name, e.Guild.MemberCount);
            await using var context = this.GetService<EspeonDbContext>();
            await context.PersistGuildAsync(e.Guild);
        }

        private async Task OnGuildLeft(LeftGuildEventArgs e) {
            this._logger.Information("Left {Guild}", e.Guild.Name);
            await using var context = this.GetService<EspeonDbContext>();
            await context.RemoveGuildAsync(e.Guild);
        }
    }
}