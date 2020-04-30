using Disqord.Events;
using Disqord.Logging;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
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
                await service.OnReadyAsync(context);
            }
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