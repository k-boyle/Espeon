using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Espeon.Logging;
using Espeon.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Espeon.DisqordImplementations {
    public class EspeonBot : DiscordBot {
        private readonly ILogger _logger;

        public EspeonBot(ILogger logger, string token, EspeonPrefixProvider prefixProvider, DiscordBotConfiguration configuration)
                : base(TokenType.Bot, token, prefixProvider, configuration) {
            this._logger = logger.ForContext("SourceContext", GetType().Name);
            Ready += OnReadyAsync;
            JoinedGuild += OnGuildJoined;
            LeftGuild += OnGuildLeft;
            Logger.MessageLogged += (sender, log) => {
                this._logger.Write(LoggingHelper.From(log.Severity), log.Exception, log.Message);
            };
        }
        
        private async Task OnReadyAsync(ReadyEventArgs e) {
            await using var context = this.GetService<EspeonDbContext>();
            foreach (var guild in e.Client.Guilds.Values) {
                this._logger.Information("Persisting {GuildName}", guild.Name);
                await context.PersistGuildAsync(guild);
            }
            this._logger.Information("Espeon is ready!");
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

        protected override async ValueTask<bool> CheckMessageAsync(CachedUserMessage message) {
            if (message.Author.IsBot) {
                return false;
            }
            
            if (!(message.Channel is IPrivateChannel)) {
                var member = message.Author as CachedMember;
                Debug.Assert(member != null);
                this._logger.Debug("Received message in {Guild} from {Author}", member.Guild.Name, member.DisplayName);
                return true;
            }

            this._logger.Debug("Received dm from {Author}", message.Author.Name);
            await message.Channel.SendMessageAsync("My programmer is too lazy to make me work in dms");
            return false;
        }
    }
}