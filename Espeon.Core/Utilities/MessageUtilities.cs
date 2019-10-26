using Discord;
using System;
using System.Linq;

namespace Espeon.Core {
	public static partial class Utilities {
		public static string GetImageUrl(IMessage message) {
			var imageUrl = "";

			if (message.Embeds.FirstOrDefault() is { } embed) {
				if (embed.Type == EmbedType.Image || embed.Type == EmbedType.Gifv) {
					imageUrl = embed.Url;
				}
			}

			if (message.Attachments.FirstOrDefault() is { } attachment) {
				var extensions = new[] {
					"png",
					"jpeg",
					"jpg",
					"gif",
					"webp"
				};

				if (extensions.Any(x => attachment.Url.EndsWith(x, StringComparison.InvariantCultureIgnoreCase))) {
					imageUrl = attachment.Url;
				}
			}

			return imageUrl;
		}
	}
}