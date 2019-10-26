using Casino.DependencyInjection;
using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Core.Commands;
using Espeon.Core.Databases.UserStore;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class Blackjack : IGame {
		private const float NormalPayout = 1f;
		private const float BlackjackPayout = 1.5f;

		public bool RunOnGatewayThread => true;

		public EspeonContext Context { get; }
		public IUserMessage Message { get; private set; }

		public IEnumerable<IEmote> Reactions =>
			new[] {
				this._hit,
				this._stop
			};

		public ICriterion<SocketReaction> Criterion { get; }

		[Inject] private readonly ICandyService _candy;
		[Inject] private readonly IEmoteService _emotes;
		[Inject] private readonly IGamesService _games;
		[Inject] private readonly IMessageService _message;
		[Inject] private readonly IServiceProvider _services;

		private Emote RareCandy => this._emotes.Collection["RareCandy"];

		private readonly IReadOnlyDictionary<string, int> _cards = new Dictionary<string, int> {
			{ "ace", 11 },
			{ "two", 2 },
			{ "three", 3 },
			{ "four", 4 },
			{ "five", 5 },
			{ "six", 6 },
			{ "seven", 7 },
			{ "eight", 8 },
			{ "nine", 9 },
			{ "ten", 10 },
			{ "jack", 10 },
			{ "queen", 10 },
			{ "king", 10 }
		};

		private readonly IReadOnlyCollection<string> _suits = new[] {
			"❤",
			"♦",
			"♣",
			"♠"
		};

		private readonly Queue<(string suit, string card, int value)> _deck;
		private List<(string suit, string card, int value)> _playerCards;
		private List<(string suit, string card, int value)> _dealerCards;

		private readonly Emoji _hit = new Emoji("➕");
		private readonly Emoji _stop = new Emoji("❌");

		private readonly int _bet;
		private readonly bool _manageMessages;

		public Blackjack(EspeonContext context, IServiceProvider services, int bet) {
			Context = context;
			Criterion = new ReactionFromSourceUser(context.User.Id);

			this._deck = new Queue<(string, string, int)>(
				(from suit in this._suits from card in this._cards select (suit, card.Key, card.Value)).OrderBy(_ =>
					services.GetService<Random>().Next()));
			this._playerCards = new List<(string suit, string card, int value)>();
			this._dealerCards = new List<(string suit, string card, int value)>();

			this._bet = bet;
			this._manageMessages = Context.Guild.CurrentUser.GetPermissions(Context.Channel).ManageMessages;
		}

		async Task<bool> IGame.StartAsync() {
			this._playerCards.Add(this._deck.Dequeue());
			this._dealerCards.Add(this._deck.Dequeue());
			this._playerCards.Add(this._deck.Dequeue());

			Message = await this._message.SendAsync(Context, x => x.Embed = BuildEmbed());

			int playerTotal = CalculateTotal(ref this._playerCards);

			if (playerTotal != 21) {
				return false;
			}

			await ((IGame) this).EndAsync();
			return true;
		}

		async Task IGame.EndAsync() {
			try {
				if (this._manageMessages) {
					await Message.RemoveAllReactionsAsync();
				}

				int playerTotal = CalculateTotal(ref this._playerCards);
				int dealerTotal = CalculateTotal(ref this._dealerCards);

				int amount;
				string description;
				Color color;

				if (playerTotal > 21) {
					//lose 

					amount = -this._bet;
					description = $"I win! You lose {Math.Abs(amount)}{RareCandy} cand{(amount == 1 ? "y" : "ies")}!";
					color = Color.Red;
				} else {
					while (dealerTotal < 17) {
						this._dealerCards.Add(this._deck.Dequeue());
						dealerTotal = CalculateTotal(ref this._dealerCards);
					}

					if (playerTotal == 21 && dealerTotal != 21) {
						//win 21

						amount = (int) (this._bet * BlackjackPayout);

						description =
							$"BLACKJACK! You win {Math.Abs(amount)}{RareCandy} cand{(amount == 1 ? "y" : "ies")}!";
						color = Color.Gold;
					} else if (dealerTotal > 21) {
						//win

						amount = (int) (this._bet * NormalPayout);

						description =
							$"I struck out! You win {Math.Abs(amount)}{RareCandy} cand{(amount == 1 ? "y" : "ies")}!";
						color = Color.Green;
					} else if (dealerTotal == playerTotal) {
						//draw

						amount = 0;

						description = "It's a draw!";
						color = Color.Orange;
					} else if (dealerTotal > playerTotal) {
						//lose

						amount = -this._bet;

						description =
							$"I win! You lose {Math.Abs(amount)}{RareCandy} cand{(amount == 1 ? "y" : "ies")}!";
						color = Color.Red;
					} else {
						//win

						amount = (int) (this._bet * NormalPayout);

						description =
							$"You have the higher score! You win {Math.Abs(amount)}{RareCandy} cand{(amount == 1 ? "y" : "ies")}!";
						color = Color.Green;
					}
				}

				var builder = new EmbedBuilder {
					Title = "Blackjack Result!",
					Fields = GetEmbedFields(),
					Description = description,
					Color = color
				};

				using var store = this._services.GetService<UserStore>();
				await this._candy.UpdateCandiesAsync(Context, store, Context.User, amount);
				await Message.ModifyAsync(x => x.Embed = builder.Build());
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		//TODO don't like => Rethink game system?
		Task IReactionCallback.InitialiseAsync() {
			return Task.CompletedTask;
		}

		Task IReactionCallback.HandleTimeoutAsync() {
			return this._games.TryLeaveGameAsync(Context);
		}

		async Task<bool> IReactionCallback.HandleCallbackAsync(SocketReaction reaction) {
			try {
				IEmote emote = reaction.Emote;

				if (emote.Equals(this._hit)) {
					this._playerCards.Add(this._deck.Dequeue());
					int playerTotal = CalculateTotal(ref this._playerCards);

					if (playerTotal >= 21) {
						await this._games.TryLeaveGameAsync(Context);
						return true;
					}
				}

				if (emote.Equals(this._stop)) {
					await this._games.TryLeaveGameAsync(Context);
					return true;
				}

				await Message.ModifyAsync(x => x.Embed = BuildEmbed());

				if (!this._manageMessages) {
					return false;
				}

				IUser user = reaction.User.GetValueOrDefault() ??
				             await Context.Client.Rest.GetUserAsync(reaction.UserId);
				await Message.RemoveReactionAsync(emote, user);
			} catch (Exception e) {
				Console.WriteLine(e);
			}

			return false;
		}

		private Embed BuildEmbed() {
			var builder = new EmbedBuilder {
				Title = "Blackjack",
				Description = $"You have bet {this._bet}{RareCandy} candies\n" +
				              $"A game of blackjack. Click {this._hit} to hit or {this._stop} to stay\n",
				Color = Color.Default,
				Fields = GetEmbedFields()
			};

			return builder.Build();
		}

		private List<EmbedFieldBuilder> GetEmbedFields() {
			var fields = new List<EmbedFieldBuilder> {
				new EmbedFieldBuilder {
					Name = $"{Context.User.GetDisplayName()}'s cards",
					Value = $"{GetCards(this._playerCards)}\n" +
					        $"For a total of: {CalculateTotal(ref this._playerCards)}"
				},
				new EmbedFieldBuilder {
					Name = $"Espeon.Core's cards",
					Value = $"{GetCards(this._dealerCards)}\n" +
					        $"For a total of: {CalculateTotal(ref this._dealerCards)}"
				}
			};

			return fields;
		}

		private static int CalculateTotal(ref List<(string suit, string card, int value)> cards) {
			int total = cards.Sum(x => x.value);

			if (total <= 21) {
				return total;
			}

			if (cards.All(x => x.card != "ace")) {
				return total;
			}

			int attemps = cards.Count(x => x.card == "ace");

			for (var a = 0; a < attemps; a++) {
				total = cards.Sum(x => x.value);

				if (total <= 21) {
					break;
				}

				for (var i = 0; i < cards.Count; i++) {
					if (cards[i].card != "ace" || cards[i].value == 1) {
						continue;
					}

					cards[i] = (cards[i].suit, cards[i].card, 1);
					break;
				}
			}

			return cards.Sum(x => x.value);
		}

		private static string GetCards(IEnumerable<(string suit, string card, int value)> cards) {
			return string.Join(", ", cards.Select(x => $"[{x.card} of {x.suit}]"));
		}
	}
}