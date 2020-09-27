using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Disqord.Extensions.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonBot : DiscordBot {
        private readonly ILogger _logger;
        private readonly LocalisationService _localisationService;

        public EspeonBot(ILogger logger, string token, EspeonPrefixProvider prefixProvider, DiscordBotConfiguration configuration)
                : base(TokenType.Bot, token, prefixProvider, configuration) {
            this._logger = logger.ForContext("SourceContext", nameof(EspeonBot));
            this._localisationService = this.GetService<LocalisationService>();
            Ready += OnReadyAsync;
            Ready += OnFirstReadyAsync;
            JoinedGuild += OnGuildJoined;
            LeftGuild += OnGuildLeft;
            CommandExecuted += OnCommandExecuted;
            CommandExecutionFailed += OnCommandExecutionFailed;
            this.GetService<EspeonScheduler>().OnError += OnSchedulerError;
            
            AddTypeParser(new UserReminderTypeParser());
            AddTypeParser(new IMessageTypeParser());
            AddModules(Assembly.GetEntryAssembly());
            AddExtensionAsync(new InteractivityExtension());
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

            if (result is TypeParseFailedResult parseFailedResult) {
                var localisationKey = this._localisationService.GetKey(parseFailedResult.Reason);
                var response = await this._localisationService.GetResponseAsync(context.Member, localisationKey);
                await context.Channel.SendMessageAsync(response);
            } else if (!(result is CommandNotFoundResult)) {
                await context.Channel.SendMessageAsync(result.Reason);
            }
            context.ServiceScope.Dispose();
        }
    }
}