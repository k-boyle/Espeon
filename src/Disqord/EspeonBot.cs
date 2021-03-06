﻿using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Disqord.Extensions.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qmmands;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonBot : DiscordBot {
        private readonly ILogger<EspeonBot> _logger;
        private readonly LocalisationService _localisationService;

        public EspeonBot(
                ILogger<EspeonBot> logger,
                IOptions<Discord> discordOptions,
                EspeonPrefixProvider prefixProvider,
                DiscordBotConfiguration configuration)
                    : base(TokenType.Bot, discordOptions.Value.Token, prefixProvider, configuration) {
            this._logger = logger; 
            this._localisationService = this.GetRequiredService<LocalisationService>();
            Ready += OnReadyAsync;
            Ready += OnFirstReadyAsync;
            JoinedGuild += OnGuildJoined;
            LeftGuild += OnGuildLeft;
            CommandExecuted += OnCommandExecuted;
            CommandExecutionFailed += OnCommandExecutionFailed;
            this.GetRequiredService<EspeonScheduler>().OnError += OnSchedulerError;
            
            AddTypeParser(new UserReminderTypeParser());
            AddTypeParser(new IMessageTypeParser());
            AddTypeParser(new ModuleTypeParser());
            AddTypeParser(new CommandTypeParser());
            AddTypeParser(new IMemberTypeParser());
            AddModules(Assembly.GetEntryAssembly(), type => type != typeof(TagModule));
            AddExtensionAsync(new InteractivityExtension());
        }

        protected override async ValueTask<bool> CheckMessageAsync(CachedUserMessage message) {
            if (message.Author.IsBot) {
                return false;
            }
            
            if (!(message.Channel is IPrivateChannel)) {
                var member = message.Author as CachedMember;
                Debug.Assert(member != null);
                this._logger.LogDebug("Received message in {Guild} from {Author}", member.Guild.Name, member.DisplayName);
                return true;
            }
            
            this._logger.LogDebug("Received dm from {Author}", message.Author.Name);
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
            this._logger.LogInformation(
                (result as ExecutionFailedResult)?.Exception,
                "Execution failed of {command} for {user} in {guild}/{channel} because of {reason}",
                context.Command?.Name,
                context.Member.DisplayName,
                context.Guild.Name,
                context.Channel.Name,
                result.Reason);

            if (result is TypeParseFailedResult parseFailedResult) {
                var localisationKey = this._localisationService.GetKey(parseFailedResult.Reason);
                var response = await this._localisationService.GetResponseAsync(context.Member.Id, context.Guild.Id, localisationKey);
                await context.Channel.SendMessageAsync(response);
            } else if (!(result is CommandNotFoundResult)) {
                await context.Channel.SendMessageAsync(result.Reason);
            }
            context.ServiceScope.Dispose();
        }
    }
}