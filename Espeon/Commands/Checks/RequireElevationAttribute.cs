using Espeon.Enums;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.Checks
{
    public class RequireElevationAttribute : RequireGuildOwnerAttribute
    {
        private readonly ElevationLevel _level;

        public RequireElevationAttribute(ElevationLevel level)
        {
            _level = level;
        }

        public override async Task<CheckResult> CheckAsync(ICommandContext originalContext, IServiceProvider provider)
        {
            var result = await base.CheckAsync(originalContext, provider);

            if(result.IsSuccessful)
                return CheckResult.Successful;

            var context = originalContext as EspeonContext;

            var currentGuild = await context!.GuildStore.GetOrCreateGuildAsync(context.Guild);

            switch (_level)
            {
                case ElevationLevel.Mod:
                    return currentGuild.Moderators.Contains(context.User.Id) ||
                           currentGuild.Admins.Contains(context.User.Id)
                        ? CheckResult.Successful
                        : CheckResult.Unsuccessful(
                            "You need to be at least a moderator of this guild to use this command");

                case ElevationLevel.Admin:
                    return currentGuild.Admins.Contains(context.User.Id)
                        ? CheckResult.Successful
                        : CheckResult.Unsuccessful("You need to be an admin to use this command");

                default:
                    return CheckResult.Unsuccessful("Something went horribly wrong");
            }
        }
    }    
}
