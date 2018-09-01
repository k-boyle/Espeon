using Discord.Commands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError($"You must {(_inGame ? "not " : "")}be in a game to use this command"));
        }
    }
}
