using Disqord.Events;
using Disqord.Logging;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Qmmands.Delegates;
using System;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonBot {
        private async Task OnReadyAsync(ReadyEventArgs e) {
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            foreach (var guild in e.Client.Guilds.Values) {
                this._logger.Information("Persisting {GuildName}", guild.Name);
                await context.PersistGuildAsync(guild);
            }
            this._logger.Information("Espeon is ready!");
        }

        private async Task OnFirstReadyAsync(ReadyEventArgs e) {
            Ready -= OnFirstReadyAsync;
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            foreach (var service in this.GetServices<IOnReadyService>()) {
                this._logger.Information("Readying {Service}", service.GetType().Name);
                await service.OnReadyAsync(context);
            }

            this._logger.Information("Adding global tags");
            var tags = await context.GetTagsAsync<GlobalTag>();
            AddModule(moduleBuilder => {
                moduleBuilder.WithName("All Global Tags").WithDescription("All the global tags");

                foreach (var tag in tags) {
                    this._logger.Debug("Adding global tag {Name}", tag.Key);
                    moduleBuilder.AddCommand(
                        context => CommandHelpers.GlobalTagCallback((EspeonCommandContext) context),
                        commandBuilder => commandBuilder.WithName(tag.Key).Aliases.Add(tag.Key));
                }
                this._logger.Debug("Created global tag module");
            });
        }

        private async Task OnGuildJoined(JoinedGuildEventArgs e) {
            this._logger.Information("Joined {Guild} with {Members} members", e.Guild.Name, e.Guild.MemberCount);
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            await context.PersistGuildAsync(e.Guild);
        }

        private async Task OnGuildLeft(LeftGuildEventArgs e) {
            this._logger.Information("Left {Guild}", e.Guild.Name);
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetService<EspeonDbContext>();
            await context.RemoveGuildAsync(e.Guild);
        }
        
        private void OnDisqordLog(object sender, MessageLoggedEventArgs e) {
            this._logger.Write(LoggingHelper.From(e.Severity), e.Exception, e.Message);
        }
        
        private void OnSchedulerError(Exception ex) {
            this._logger.Error("Error occured inside of the scheduler", ex);
        }

        private Task OnCommandExecuted(CommandExecutedEventArgs e) {
            var context = (EspeonCommandContext) e.Context;
            this._logger.Information(
                "Executed {Command} for {User} in {Guild}/{Channel}",
                context.Command.Name,
                context.Member.DisplayName,
                context.Guild.Name,
                context.Channel.Name);
            context.ServiceScope.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnCommandExecutionFailed(CommandExecutionFailedEventArgs e) {
            var context = (EspeonCommandContext) e.Context;
            await ExecutionFailedAsync(context, e.Result);
        }
    }
}