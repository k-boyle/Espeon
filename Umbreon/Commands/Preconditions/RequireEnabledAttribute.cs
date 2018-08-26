using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Attributes;
using Umbreon.Services;

namespace Umbreon.Commands.Preconditions
{
    public class RequireEnabledAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var database = services.GetService<DatabaseService>();
            var guild = database.GetGuild(context);
            if (!guild.DisabledModules.Any())
                return Task.FromResult(PreconditionResult.FromSuccess());
            var moduleType = command.Module.Attributes.FirstOrDefault(x => x is ModuleTypeAttribute) as ModuleTypeAttribute;
            return guild.DisabledModules.Contains(moduleType.Type) ? Task.FromResult(PreconditionResult.FromError(new FailedResult("This module has been disabled", false, CommandError.UnmetPrecondition))) : Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
