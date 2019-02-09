using Espeon.Commands.Games;
using Espeon.Services;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    /*
     * Blackjack
     * Coinflip
     * Duel
     * Minesweeper
     */

    [Name("Games")]
    public class Games : EspeonBase
    {
        public GamesService GameService { get; set; }

        //TODO typereader
        [Command("blackjack")]
        public async Task StartBlackjackAsync(int bet = 0)
        {
            if (bet < 0)
            {
                await SendNotOkAsync("fuck off");
                return;
            }

            var bj = new Blackjack(Context, Services, bet);

            var result = await GameService.TryStartGameAsync(Context, bj, TimeSpan.FromMinutes(5));

            if (!result)
            {
                await SendNotOkAsync("You're in a game already or some shit lol");
            }
        }
    }
}
