using System;
using Discord.Commands;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Attributes;
using Umbreon.Commands.Games;
using Umbreon.Commands.ModuleBases;
using Umbreon.Commands.Preconditions;
using Umbreon.Core;
using Umbreon.Services;

namespace Umbreon.Commands.Modules
{
    [Name("Games")]
    [Summary("Some small games to play in the guild")]
    [ModuleType(Module.Games)]
    public class Games : UmbreonBase
    {
        private readonly GamesService _games;

        public Games(GamesService games)
        {
            _games = games;
        }

        [Command("blackjack")]
        [Summary("Start a game of blackjack")]
        [Name("Blackjack")]
        [Usage("blackjack")]
        [RequireGame(false)]
        public Task BlackJack()
            => _games.StartGameAsync(Context.User.Id, new Blackjack(Context, Message, Interactive, _games, Services.GetService<Random>()));
    }
}
