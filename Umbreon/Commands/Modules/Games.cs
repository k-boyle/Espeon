using Discord.Commands;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.Games;
using Umbreon.Commands.ModuleBases;
using Umbreon.Commands.Preconditions;
using Umbreon.Core;
using Umbreon.Services;
using Remarks = Umbreon.Attributes.RemarksAttribute;

namespace Umbreon.Commands.Modules
{
    [Name("Games")]
    [Summary("Some small games to play in the guild")]
    [ModuleType(Module.Games)]
    public class Games : UmbreonBase
    {
        private readonly GamesService _games;
        private readonly CandyService _candy;

        public Games(GamesService games, CandyService candy)
        {
            _games = games;
            _candy = candy;
        }

        [Command("blackjack")]
        [Alias("bj")]
        [Summary("Start a game of blackjack")]
        [Name("Blackjack")]
        [Usage("blackjack 10")]
        [RequireGame(false)]
        public async Task BlackJack(
            [Name("Amount To Bet")]
            [Summary("The amount of rare candies you want to bet.")]
            [Remarks("Don't specify for no bet")] uint amount = 0)
        {
            if (amount > _candy.GetCandies(Context.User.Id))
            {
                await SendMessageAsync("You don't have enough rare candies to bet");
                return;
            }

            await _games.StartGameAsync(Context.User.Id, new Blackjack(Context, (int)amount, Services));
        }

        [Command("blackjack all")]
        [Alias("bj all")]
        [Summary("Start a game of blackjack betting all your candies")]
        [Name("Blackjack")]
        [Usage("blackjack all")]
        [RequireGame(false)]
        public Task BlackJackAll()
            => _games.StartGameAsync(Context.User.Id, new Blackjack(Context, _candy.GetCandies(Context.User.Id), Services));
    }
}
