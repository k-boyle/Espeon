using Espeon.Core;
using Espeon.Core.Services;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	/*
	* Blackjack
	* Coinflip
	* Duel
	* Minesweeper
	*/

	[Name("Games")]
	[Description("Games that can be played with the bot")]
	public class Games : EspeonModuleBase {
		public ICandyService Candy { get; set; }
		public Config Config { get; set; }
		public IEmoteService Emotes { get; set; }
		public IGamesService<IGame> GameService { get; set; }
		public Random Random { get; set; }

		[Command("blackjack")]
		[Name("Blackjack")]
		[Description("Starts a game of blackjack, gamble safe kids")]
		public async Task StartBlackjackAsync([OverrideTypeParser(typeof(CandyTypeParser))] int bet = 0) {
			var bj = new Blackjack(Context, Services, bet);

			bool result = await GameService.TryStartGameAsync(User.Id, bj, TimeSpan.FromMinutes(5));

			if (!result) {
				await SendNotOkAsync(0);
			}
		}

		[Command("Coinflip")]
		[Name("Coinflip")]
		[Description("Flip a coin the specified amount of times")]
		public Task CoinFlipAsync(Face choice, [OverrideTypeParser(typeof(CandyTypeParser))] int bet = 0) {
			Face flip = Random.Next(100) > 50 ? Face.Heads : Face.Tails;
			bool win = flip == choice;
			var payout = (int) (bet * Config.CoinFlip);
			string plural = payout == 1 ? "y" : "ies";

			return Task.WhenAll(
				win
					? SendOkAsync(0, payout, Emotes.Collection["RareCandy"], plural)
					: SendNotOkAsync(1, payout, Emotes.Collection["RareCandy"], plural),
				Candy.UpdateCandiesAsync(Context.UserStore, Client.CurrentUser, User, (win ? 1 : -1) * payout));
		}
	}
}