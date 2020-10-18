using Disqord.Events;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonBot {
        private async Task OnReadyAsync(ReadyEventArgs e) {
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            foreach (var guild in e.Client.Guilds.Values) {
                this._logger.LogInformation("Persisting {guildName}", guild.Name);
                await context.PersistGuildAsync(guild);
            }
            
            this._logger.LogInformation("Espeon ready");
        }

        private async Task OnFirstReadyAsync(ReadyEventArgs args) {
            Ready -= OnFirstReadyAsync;
            CSharpScript.Create("").Compile();
            this._logger.LogDebug("Roslyn initialised");
            
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();

            await OnReadyServicesAsync(context);
            await CommandHelper.AddGlobalTagsAsync(context, this, this._logger);

            this._logger.LogInformation("Espeon on first ready executed");
        }

        private async Task OnReadyServicesAsync(EspeonDbContext context) {
            var onReadyServices = this.GetServices<IOnReadyService>();

            foreach (var service in onReadyServices) {
                await service.OnReadyAsync(context);
            }
        }

        private async Task OnGuildJoined(JoinedGuildEventArgs e) {
            this._logger.LogInformation("Joined {guild} with {members} members", e.Guild.Name, e.Guild.MemberCount);
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.PersistGuildAsync(e.Guild);
        }

        private async Task OnGuildLeft(LeftGuildEventArgs e) {
            this._logger.LogInformation("Left {guild}", e.Guild.Name);
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.RemoveGuildAsync(e.Guild);
        }
        
        private void OnSchedulerError(Exception ex) {
            this._logger.LogError("Error occured inside of the scheduler", ex);
        }

        private Task OnCommandExecuted(CommandExecutedEventArgs e) {
            var context = (EspeonCommandContext) e.Context;
            this._logger.LogInformation(
                "Executed {command} for {user} in {guild}/{channel}",
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