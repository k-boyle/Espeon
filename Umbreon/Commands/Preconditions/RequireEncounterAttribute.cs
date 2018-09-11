using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Services;

namespace Umbreon.Commands.Preconditions
{
    public class RequireEncounterAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo _,
            IServiceProvider services)
            => Task.FromResult(!services.GetService<PokemonPlayerService>().InEncounter(context.User.Id) ? 
                PreconditionResult.FromSuccess() :  
                PreconditionResult.FromError(new FailedResult("You are already in an encounter", CommandError.UnmetPrecondition)));
    }
}
