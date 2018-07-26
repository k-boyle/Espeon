using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Attributes;
using Umbreon.Results;
using Umbreon.Services;

namespace Umbreon.Preconditions
{
    public class RequireEnabled : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var database = services.GetService<DatabaseService>();
            var guild = database.GetGuild(context);
            if (!guild.DisabledModules.Any())
                return Task.FromResult(PreconditionResult.FromSuccess());
            var moduleType = command.Module.Attributes.FirstOrDefault(x => x is ModuleType) as ModuleType;
            return guild.DisabledModules.Contains(moduleType.Type) ? Task.FromResult(PreconditionResult.FromError(new FailedResult("This module has been disabled", false, CommandError.UnmetPrecondition))) : Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
