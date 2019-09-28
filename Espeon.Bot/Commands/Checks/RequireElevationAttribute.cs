using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class RequireElevationAttribute : RequireGuildOwnerAttribute
    {
        private readonly ElevationLevel _level;

        public RequireElevationAttribute(ElevationLevel level)
        {
            _level = level;
        }

        public override async ValueTask<CheckResult> CheckAsync(EspeonContext context, IServiceProvider provider)
        {
            var result = await base.CheckAsync(context, provider);

            if(result.IsSuccessful)
                return CheckResult.Successful;

            var response = provider.GetService<IResponseService>();

            var currentGuild = context.CurrentGuild;

            var p = context.Invoker.ResponsePack;

            switch (_level)
            {
                case ElevationLevel.Mod:
                    return currentGuild.Moderators.Contains(context.User.Id) ||
                           currentGuild.Admins.Contains(context.User.Id)
                        ? CheckResult.Successful
                        : CheckResult.Unsuccessful(response.GetResponse(this, p, 0));

                case ElevationLevel.Admin:
                    return currentGuild.Admins.Contains(context.User.Id)
                        ? CheckResult.Successful
                        : CheckResult.Unsuccessful(response.GetResponse(this, p, 1));

                default:
                    return CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
            }
        }
    }
}
