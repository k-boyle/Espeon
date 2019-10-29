using Disqord;
using Disqord.Rest;
using Humanizer;
using System;
using System.Threading.Tasks;

namespace Espeon.Core {
	public static partial class Utilities {
		public static Task<LocalEmbed> QuoteFromStringAsync(DiscordClient client, string content) {
			string[] split = content.Split('/', StringSplitOptions.RemoveEmptyEntries);

			if (split.Length != 6 || !ulong.TryParse(split[4], out ulong id) ||
			    !(client.GetChannel(id) is CachedTextChannel channel) || !ulong.TryParse(split[5], out id)) {
				return Task.FromResult<LocalEmbed>(null);
			}

			return QuoteFromMessageIdAsync(channel, id);
		}

		public static async Task<LocalEmbed> QuoteFromMessageIdAsync(CachedTextChannel channel, ulong id) {
			if (!(await channel.GetMessageAsync(id) is RestUserMessage message)) {
				return null;
			}

			string imageUrl = GetImageUrl(message);

			LocalEmbedBuilder builder = BaseEmbed(message.Author, imageUrl, message.Content).AddField("\u200b",
				$"Sent {message.Id.CreatedAt.Humanize()} " + $"in {channel.Guild.Name} / <#{channel.Id}> / " +
				Markdown.MaskedUrl(message.Id.ToString(), await message.GetJumpUrlAsync()));

			return builder.Build();
		}

		private static LocalEmbedBuilder BaseEmbed(IUser author, string imageUrl, string content) {
			return new LocalEmbedBuilder {
				Author = new LocalEmbedAuthorBuilder() {
					IconUrl = author.GetAvatarUrl(),
					Name = (author as IMember)?.DisplayName ?? author.Name
				},
				ImageUrl = imageUrl,
				Description = content,
				Color = EspeonColor
			};
		}
	}
}