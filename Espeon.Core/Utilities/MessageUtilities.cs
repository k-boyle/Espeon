using Disqord;
using System;
using System.Linq;

namespace Espeon.Core {
	public static partial class Utilities {
		public static string GetImageUrl(IUserMessage message) {
			var imageUrl = "";

			if (message.Embeds.FirstOrDefault() is { } embed) {
				if (embed.Type == "image" || embed.Type == "gifv") {
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