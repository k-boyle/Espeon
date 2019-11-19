using Disqord;
using Disqord.Events;
using Disqord.Rest;
using Espeon.Core;
using Espeon.Core.Services;
using Kommon.Common;
using Kommon.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class QuoteService : BaseService<InitialiseArgs>, IQuoteService {
		private const string RegexString = @"(?:https://(?:canary.)?discordapp.com/channels/[\d]+/[\d]+/[\d]+)";

		private static readonly Regex Regex = new Regex(RegexString, RegexOptions.Compiled);
		private static readonly LocalEmoji QuoteEmote = new LocalEmoji("🗨");
		private static readonly TimeSpan MessageLifeTime = TimeSpan.FromMinutes(10);

		private readonly ConcurrentDictionary<ulong, ulong> _lastJumpUrlQuotes;
		private readonly ConcurrentQueue<ulong> _quoteReactions;

		[Inject] private readonly DiscordClient _client;
		[Inject] private readonly IEventsService _events;
		[Inject] private readonly TaskQueue _scheduler;

		public QuoteService(IServiceProvider services) : base(services) {
			this._lastJumpUrlQuotes = new ConcurrentDictionary<ulong, ulong>(2, 10);
			this._quoteReactions = new ConcurrentQueue<ulong>();

			this._client.MessageReceived += args => this._events.RegisterEvent(() => {
				if (args.Message.Channel is IDmChannel) {
					return Task.CompletedTask;
				}

				CacheJumpUrl(args.Message);
				return Task.CompletedTask;
			});

			this._client.ReactionAdded += HandleReactionAsync;
		}

		private void CacheJumpUrl(CachedMessage message) {
			Match match = Regex.Match(message.Content);

			if (match.Success) {
				this._lastJumpUrlQuotes[message.Channel.Id] = message.Id;
			}
		}

		private async Task HandleReactionAsync(ReactionAddedEventArgs args) {
			if (!args.Emoji.Equals(QuoteEmote) || this._quoteReactions.Any(x => x == args.Message.Id) ||
			    args.Channel is IDmChannel) {
				return;
			}

			IMessage message = await args.Message.GetOrDownloadAsync<IMessage, CachedMessage, RestMessage>();

			if (message is null) {
				return;
			}

			LocalEmbed embed = await Utilities.QuoteFromStringAsync(this._client, message.Content);

			if (embed is null) {
				return;
			}

			await ((IMessageChannel) this._client.GetChannel(message.ChannelId)).SendMessageAsync(string.Empty, embed: embed);

			this._quoteReactions.Enqueue(args.Message.Id);

			this._scheduler.ScheduleTask(this._quoteReactions, MessageLifeTime, state => {
				state.TryDequeue(out _);
				return Task.CompletedTask;
			});
		}

		bool IQuoteService.TryGetLastJumpMessage(ulong channelId, out ulong messageId) {
			return this._lastJumpUrlQuotes.TryGetValue(channelId, out messageId);
		}
	}
}