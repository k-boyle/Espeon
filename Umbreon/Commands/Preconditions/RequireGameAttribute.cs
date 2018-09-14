using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Umbreon.Services;

namespace Umbreon.Commands.Preconditions
{
    public class RequireGameAttribute : PreconditionAttribute
    {
        private readonly bool _inGame;

        public RequireGameAttribute(bool inGame)
        {
            _inGame = inGame;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var games = services.GetService<GamesService>();
            return Task.FromResult(games.InGame(context) == _inGame
                ? PreconditionResult.FromSuccess(command)
                : PreconditionResult.FromError(command, $"You must {(!_inGame ? "not " : "")}be in a game to use this command"));
        }
    }
}
