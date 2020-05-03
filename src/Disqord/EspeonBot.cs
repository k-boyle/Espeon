using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonBot : DiscordBot {
        private readonly ILogger _logger;

        public EspeonBot(ILogger logger, string token, EspeonPrefixProvider prefixProvider, DiscordBotConfiguration configuration)
                : base(TokenType.Bot, token, prefixProvider, configuration) {
            this._logger = logger.ForContext("SourceContext", nameof(EspeonBot));
            Ready += OnReadyAsync;
            Ready += OnFirstReadyAsync;
            JoinedGuild += OnGuildJoined;
            LeftGuild += OnGuildLeft;
            Logger.MessageLogged += OnDisqordLog;
            CommandExecuted += OnCommandExecuted;
            CommandExecutionFailed += OnCommandExecutionFailed;
            this.GetService<EspeonScheduler>().OnError += OnSchedulerError;
            
            AddTypeParser(new UserReminderTypeParser());
            AddModules(Assembly.GetEntryAssembly());
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
            var scope = this.CreateScope();
            return new ValueTask<DiscordCommandContext>(new EspeonCommandContext(scope, this, prefix, message));
        }

        protected override async ValueTask AfterExecutedAsync(IResult result, DiscordCommandContext context) {
            if (result is FailedResult failedResult && !(result is ExecutionFailedResult)) {
                await ExecutionFailedAsync((EspeonCommandContext) context, failedResult);
            }
        }
        
        private async Task ExecutionFailedAsync(EspeonCommandContext context, FailedResult result) {
            this._logger.Information(
                (result as ExecutionFailedResult)?.Exception,
                "Execution failed of {command} for {user} in {guild}/{channel} because of {reason}",
                context.Command?.Name,
                context.Member.DisplayName,
                context.Guild.Name,
                context.Channel.Name,
                result.Reason);
            //temp
            await context.Channel.SendMessageAsync(result.Reason);
            context.ServiceScope.Dispose();
        }
    }
}