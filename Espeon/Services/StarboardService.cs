using Casino.DependencyInjection;
using Casino.Discord;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Databases;
using Espeon.Core.Databases.GuildStore;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class StarboardService : BaseService<InitialiseArgs>, IStarboardService {
		[Inject] private readonly DiscordSocketClient _client;
		[Inject] private readonly IEventsService _events;
		[Inject] private readonly IServiceProvider _services;
		[Inject] private readonly ILogService _logger;

		private static Emoji Star => Utilities.Star;

		public StarboardService(IServiceProvider services) : base(services) {
			this._client.ReactionAdded += (cache, channel, reaction) =>
				this._events.RegisterEvent(() => ReactionAddedAsync(cache, channel, reaction));

			this._client.ReactionRemoved += (cache, channel, reaction) =>
				this._events.RegisterEvent(() => ReactionRemovedAsync(cache, channel, reaction));
		}

		private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
			SocketReaction reaction) {
			if (!(channel is SocketTextChannel textChannel)) {
				return;
			}

			if (!reaction.Emote.Equals(Star)) {
				return;
			}

			IUserMessage message = await msg.GetOrDownloadAsync();

			if (reaction.UserId == message.Author.Id) {
				return;
			}

			try {
				using var guildStore = this._services.GetService<GuildStore>();
				Guild guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

				if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is SocketTextChannel starChannel)) {
					return;
				}

				int count = message.Reactions[Star].ReactionCount;

				IEnumerable<IUser> flat = await message.GetReactionUsersAsync(Star, count).FlattenAsync();
				IUser[] users = flat.Where(x => x.Id != message.Author.Id).ToArray();

				count = users.Length;

				if (count < guild.StarLimit) {
					return;
				}

				StarredMessage foundMessage =
					guild.StarredMessages.FirstOrDefault(x => x.Id == message.Id || x.StarboardMessageId == message.Id);

				string m =
					$"{Star} **{count}** - {(message.Author as IGuildUser).GetDisplayName()} in <#{message.Channel.Id}>";

				if (foundMessage is null) {
					Embed embed = Utilities.BuildStarMessage(message);

					RestUserMessage newStar = await starChannel.SendMessageAsync(m, embed: embed);

					var toAdd = new StarredMessage {
						AuthorId = message.Author.Id,
						ChannelId = message.Channel.Id,
						Id = message.Id,
						StarboardMessageId = newStar.Id,
						ReactionUsers = users.Select(x => x.Id).ToList(),
						ImageUrl = embed.Image?.Url,
						Content = message.Content
					};

					guild.StarredMessages.Add(toAdd);
				} else {
					if (foundMessage.ReactionUsers.Contains(reaction.UserId)) {
						return;
					}

					foundMessage.ReactionUsers.Add(reaction.UserId);

					if (await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) is IUserMessage
						fetchedMessage) {
						await fetchedMessage.ModifyAsync(x => x.Content = m);
					}
				}

				guildStore.Update(guild);

				await guildStore.SaveChangesAsync();
			} catch (Exception ex) {
				this._logger.Log(Source.Starboard, Severity.Error, string.Empty, ex);
			}
		}

		private async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
			SocketReaction reaction) {
			if (!(channel is SocketTextChannel textChannel)) {
				return;
			}

			if (!reaction.Emote.Equals(Star)) {
				return;
			}

			try {
				using var guildStore = this._services.GetService<GuildStore>();
				Guild guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

				if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is { } starChannel)) {
					return;
				}

				IUserMessage message = await msg.GetOrDownloadAsync();

				StarredMessage foundMessage =
					guild.StarredMessages.Find(x => x.Id == message.Id || x.StarboardMessageId == message.Id);

				if (foundMessage is null) {
					return;
				}

				if (!foundMessage.ReactionUsers.Remove(reaction.UserId)) {
					return;
				}

				int count = message.Reactions.ContainsKey(Star) ? message.Reactions[Star].ReactionCount : 0;

				var starMessage = await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) as IUserMessage;

				if (starMessage is null || count < guild.StarLimit) {
					_ = starMessage?.DeleteAsync();

					guild.StarredMessages.Remove(foundMessage);
				} else {
					string m =
						$"{Star} **{count}** - {(message.Author as IGuildUser)?.GetDisplayName()} in <#{message.Channel.Id}>";

					await starMessage.ModifyAsync(x => x.Content = m);
				}

				guildStore.Update(guild);

				await guildStore.SaveChangesAsync();
			} catch (Exception ex) {
				this._logger.Log(Source.Starboard, Severity.Error, string.Empty, ex);
			}
		}
	}
}