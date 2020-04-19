using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonBot : DiscordBot {
        private readonly ILogger _logger;

        public EspeonBot(ILogger logger, string token, EspeonPrefixProvider prefixProvider, DiscordBotConfiguration configuration)
                : base(TokenType.Bot, token, prefixProvider, configuration) {
            this._logger = logger.ForContext("SourceContext", typeof(EspeonBot).Name);
            Ready += OnReadyAsync;
            Ready += OnFirstReadyAsync;
            JoinedGuild += OnGuildJoined;
            LeftGuild += OnGuildLeft;
            Logger.MessageLogged += (sender, log) => {
                this._logger.Write(LoggingHelper.From(log.Severity), log.Exception, log.Message);
            };
            this.GetService<EspeonScheduler>().OnError += OnSchedulerError;
            
            AddTypeParser(new UserReminderTypeParser());
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

        protected override ValueTask<DiscordCommandContext> GetCommandContextAsync(CachedUserMessage message, IPrefix prefix) {
            return new ValueTask<DiscordCommandContext>(new EspeonCommandContext(this, prefix, message));
        }
    }
}