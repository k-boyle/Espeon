using Disqord.Events;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon {
    public partial class EspeonBot {
        private async Task OnReadyAsync(ReadyEventArgs e) {
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            foreach (var guild in e.Client.Guilds.Values) {
                this._logger.Information("Persisting {guildName}", guild.Name);
                await context.PersistGuildAsync(guild);
            }
        }

        private async Task OnFirstReadyAsync(ReadyEventArgs e) {
            Ready -= OnFirstReadyAsync;
            CSharpScript.Create("").Compile();
            this._logger.Information("Roslyn initialised");
            
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();

            this._logger.Information("Adding global tags");
            var tags = await context.GetTagsAsync<GlobalTag>();
            AddModule(moduleBuilder => {
                moduleBuilder.WithName("All Global Tags").WithDescription("All the global tags");

                foreach (var tag in tags) {
                    this._logger.Debug("Adding global tag {name}", tag.Key);
                    moduleBuilder.AddCommand(
                        context => CommandHelpers.GlobalTagCallbackAsync((EspeonCommandContext) context),
                        commandBuilder => commandBuilder.WithName(tag.Key).Aliases.Add(tag.Key));
                }
                this._logger.Debug("Created global tag module");
            });
        }

        private async Task OnGuildJoined(JoinedGuildEventArgs e) {
            this._logger.Information("Joined {guild} with {members} members", e.Guild.Name, e.Guild.MemberCount);
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.PersistGuildAsync(e.Guild);
        }

        private async Task OnGuildLeft(LeftGuildEventArgs e) {
            this._logger.Information("Left {guild}", e.Guild.Name);
            using var scope = this.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EspeonDbContext>();
            await context.RemoveGuildAsync(e.Guild);
        }
        
        private void OnSchedulerError(Exception ex) {
            this._logger.Error("Error occured inside of the scheduler", ex);
        }

        private Task OnCommandExecuted(CommandExecutedEventArgs e) {
            var context = (EspeonCommandContext) e.Context;
            this._logger.Information(
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