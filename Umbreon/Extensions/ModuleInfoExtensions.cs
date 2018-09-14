using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Umbreon.Extensions
{
    public static class ModuleInfoExtensions
    {
        public static async Task<PreconditionResult> CheckPermissionsAsync(this ModuleInfo module, ICommandContext context, IServiceProvider services)
        {
            async Task<PreconditionResult> CheckGroups(IEnumerable<PreconditionAttribute> preconditions, string type)
            {
                foreach (var preconditionGroup in preconditions.GroupBy(x => x.Group, StringComparer.Ordinal))
                {
                    if (preconditionGroup.Key is null)
                    {
                        foreach (var precondition in preconditionGroup)
                        {
                            var result = await precondition.CheckPermissionsAsync(context, null, services);
                            if (!result.IsSuccess)
                                return result;
                        }
                    }
                    else
                    {
                        var results = new List<PreconditionResult>();
                        foreach(var precondition in preconditionGroup)
                            results.Add(await precondition.CheckPermissionsAsync(context, null, services));

                        if (!results.Any(x => x.IsSuccess))
                            return PreconditionGroupResult.FromError(null,
                                $"{type} precondition group {preconditionGroup.Key} failed", results);
                    }
                }
                return PreconditionResult.FromSuccess(null); //having null is hacky but better than selecting a random command
            }

            var moduleResult = await CheckGroups(module.Preconditions, "Module");
            return !moduleResult.IsSuccess ? moduleResult : PreconditionResult.FromSuccess(null);
        }
    }
}
