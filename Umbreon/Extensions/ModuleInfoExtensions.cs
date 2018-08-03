using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Umbreon.Extensions
{
    public static class ModuleInfoExtensions
    {
        public static async Task<PreconditionResult> CheckPermissionsAsync(this ModuleInfo module, ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var results = new List<PreconditionResult>();
            foreach (var precon in module.Preconditions)
            {
                if (precon.Group is null)
                {
                    results.Add(await precon.CheckPermissionsAsync(context, command, services));
                    continue;
                }

                var grouped = module.Preconditions.Where(x => x.Group == precon.Group);
                foreach (var pre in grouped)
                {
                    var res = await pre.CheckPermissionsAsync(context, null, services);
                    if (!res.IsSuccess) continue;
                    results.Add(res);
                    break;
                }
            }

            return results.Any(x => !x.IsSuccess) ? PreconditionResult.FromError("Precondition not met") : PreconditionResult.FromSuccess();
        }
    }
}
