using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Espeon.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Espeon.DisqordImplementations {
    public class EspeonBot : DiscordBot {
        public EspeonBot(string token, EspeonPrefixProvider prefixProvider, DiscordBotConfiguration configuration)
                : base(TokenType.Bot, token, prefixProvider, configuration) {
            Ready += OnReadyAsync;
            JoinedGuild += OnGuildJoined;
            LeftGuild += OnGuildLeft;
        }
        
        private async Task OnReadyAsync(ReadyEventArgs e) {
            await using var context = this.GetService<EspeonDbContext>();
            foreach (var guild in e.Client.Guilds.Values) {
                await context.PersistGuildAsync(guild);
            }
        }

        private async Task OnGuildJoined(JoinedGuildEventArgs e) {
            await using var context = this.GetService<EspeonDbContext>();
            await context.PersistGuildAsync(e.Guild);
        }

        private async Task OnGuildLeft(LeftGuildEventArgs e) {
            await using var context = this.GetService<EspeonDbContext>();
            await context.RemoveGuildAsync(e.Guild);
        }
    }
}