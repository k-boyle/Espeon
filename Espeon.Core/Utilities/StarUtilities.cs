using Disqord;
using System.Threading.Tasks;

namespace Espeon.Core {
	public static partial class Utilities {
		public static async Task<LocalEmbed> BuildStarMessageAsync(IMessage message) {
			string imageUrl = message is IUserMessage msg ? GetImageUrl(msg) : "";

			return BuildStarMessage(message.Author, message.Content, await message.GetJumpUrlAsync(), imageUrl);
		}

		public static LocalEmbed BuildStarMessage(IUser user, string content, string jumpUrl, string imageUrl = null) {
			LocalEmbedBuilder builder =
				new LocalEmbedBuilder {
					Author = new LocalEmbedAuthorBuilder() {
						Name = (user as IMember)?.DisplayName ?? user.Name,
						IconUrl = user.GetAvatarUrl()
					},
					Description = content,
					Color = Color.Gold
				}.AddField("\u200b", Markdown.MaskedUrl("Original Message", jumpUrl));

			if (!string.IsNullOrEmpty(imageUrl)) {
				builder.WithImageUrl(imageUrl);
			}

			return builder.Build();
		}

		public static string BuildJumpUrl(ulong guildId, ulong channelId, ulong messageId) {
			const string baseUrl = "https://discordapp.com/channels";
			return $"{baseUrl}/{guildId}/{channelId}/{messageId}";
		}

		public static readonly LocalEmoji Star = new LocalEmoji("⭐");
	}
}