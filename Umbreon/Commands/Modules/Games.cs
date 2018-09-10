using Discord.Commands;
using Discord.WebSocket;
using System;
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
        private readonly Random _random;

        public Games(GamesService games, CandyService candy, Random random)
        {
            _games = games;
            _candy = candy;
            _random = random;
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

        [Command("coinflip")]
        [Alias("cf")]
        [Summary("Flip a coin")]
        [Name("Coin Flip")]
        [Usage("coinflip heads 100")]
        [RequireGame(false)]
        public async Task CoinFlip(
            [Name("Face")]
            [Summary("Heads or tails")] Face choice,
            [Name("Amount")]
            [Summary("The amount of candies you want to bet")] uint amount)
        {
            if (amount > _candy.GetCandies(Context.User.Id))
            {
                await SendMessageAsync("You don't have enough rare candies to bet");
                return;
            }

            var flip = _random.Next(100) > 50 ? Face.Heads : Face.Tails;

            _candy.UpdateCandies(Context.User.Id, false, flip == choice ? (int)(0.5 * amount) : -(int)amount);
            await SendMessageAsync($"It was {flip}! {(flip == choice ? "You win!" : "You lose!")}");
        }

        [Command("coinflip all")]
        [Alias("cf all")]
        [Summary("Flip a coin")]
        [Name("Coin Flip")]
        [Usage("coinflip all heads")]
        [RequireGame(false)]
        public Task CoinFlipAll(
            [Name("Face")]
            [Summary("Heads or tails")] Face choice)
            => CoinFlip(choice, (uint)_candy.GetCandies(Context.User.Id));

        [Command("duel")]
        [Summary("Challenge someone to a duel")]
        [Name("Duel")]
        [Usage("Duel Umbreon 100")]
        [RequireGame(false)]
        public async Task Duel(
            [Name("User")]
            [Summary("The user you want to duel")] SocketGuildUser user,
            [Name("Amount")]
            [Summary("The amount you want to wager")] uint amount = 0)
        {
            if (amount > _candy.GetCandies(Context.User.Id))
            {
                await SendMessageAsync("You don't have enough rare candies to bet");
                return;
            }

            await _games.StartGameAsync(Context.User.Id, new Duel(Context, user, (int)amount, Services));
        }
    }
}
