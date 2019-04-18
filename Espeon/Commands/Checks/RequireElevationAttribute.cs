using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class RequireElevationAttribute : RequireGuildOwnerAttribute
    {
        private readonly ElevationLevel _level;

        public RequireElevationAttribute(ElevationLevel level)
        {
            _level = level;
        }

        public override async ValueTask<CheckResult> CheckAsync(CommandContext originalContext, IServiceProvider provider)
        {
            var result = await base.CheckAsync(originalContext, provider);

            if(result.IsSuccessful)
                return CheckResult.Successful;

            var context = (EspeonContext)originalContext;

            var currentGuild = context.CurrentGuild;

            var resp = new Dictionary<ResponsePack, string[]>
            {
                [ResponsePack.Default] = new []
                {
                    "You need to be at least a moderator of this guild to use this command",
                    "You need to be an admin of this guild to use this command",
                    "something went horribly wrong"
                },
                [ResponsePack.owo] = new []
                {
                    "ownno >,< u need to be a mod",
                    "ownnooo u need to bwe a daddy",
                    "sumting went hirribly wong"
                }
            };

            var p = context.Invoker.ResponsePack;

            switch (_level)
            {
                case ElevationLevel.Mod:
                    return currentGuild.Moderators.Contains(context.User.Id) ||
                           currentGuild.Admins.Contains(context.User.Id)
                        ? CheckResult.Successful
                        : CheckResult.Unsuccessful(resp[p][0]);

                case ElevationLevel.Admin:
                    return currentGuild.Admins.Contains(context.User.Id)
                        ? CheckResult.Successful
                        : CheckResult.Unsuccessful(resp[p][1]);

                default:
                    return CheckResult.Unsuccessful(resp[p][2]);
            }
        }
    }    
}
