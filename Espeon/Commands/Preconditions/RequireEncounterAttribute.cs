using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Espeon.Services;

namespace Espeon.Commands.Preconditions
{
    public class RequireEncounterAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
            => Task.FromResult(!services.GetService<PokemonPlayerService>().InEncounter(context.User.Id) ? 
                PreconditionResult.FromSuccess(command) :  
                PreconditionResult.FromError(command, "You are already in an encounter"));
    }
}
