using Espeon.Commands.Games;
using Espeon.Commands.TypeParsers;
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
        
        [Command("blackjack")]
        [Name("Blackjack")]
        public async Task StartBlackjackAsync([OverrideTypeParser(typeof(CandyTypeParser))] int bet = 0)
        {
            var bj = new Blackjack(Context, Services, bet);

            var result = await GameService.TryStartGameAsync(Context, bj, TimeSpan.FromMinutes(5));

            if (!result)
            {
                await SendNotOkAsync(0);
            }
        }
    }
}
