using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Humanizer;
using System;
using System.Threading.Tasks;

namespace Espeon.Core {
	public static partial class Utilities {
		public static Task<Embed> QuoteFromStringAsync(DiscordSocketClient client, string content) {
			string[] split = content.Split('/', StringSplitOptions.RemoveEmptyEntries);

			if (split.Length != 6 || !ulong.TryParse(split[4], out ulong id) ||
			    !(client.GetChannel(id) is ITextChannel channel) || !ulong.TryParse(split[5], out id)) {
				return Task.FromResult<Embed>(null);
			}

			return QuoteFromMessageIdAsync(channel, id);
		}

		public static async Task<Embed> QuoteFromMessageIdAsync(ITextChannel channel, ulong id) {
			IMessage message = await channel.GetMessageAsync(id);

			if (message is null) {
				return null;
			}

			string imageUrl = GetImageUrl(message);

			EmbedBuilder builder = BaseEmbed(message.Author, imageUrl, message.Content).AddField("\u200b",
				$"Sent {message.CreatedAt.Humanize()} " + $"in {channel.Guild.Name} / <#{message.Channel.Id}> / " +
				$"{Format.Url($"{message.Id}", message.GetJumpUrl())}");

			return builder.Build();
		}

		private static EmbedBuilder BaseEmbed(IUser author, string imageUrl, string content) {
			return new EmbedBuilder {
				Author = new EmbedAuthorBuilder {
					IconUrl = author.GetAvatarOrDefaultUrl(),
					Name = (author as IGuildUser)?.GetDisplayName() ?? author.Username
				},
				ImageUrl = imageUrl,
				Description = content,
				Color = EspeonColor
			};
		}
	}
}