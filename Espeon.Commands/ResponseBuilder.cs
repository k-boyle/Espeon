using Disqord;
using Humanizer;
using System;

namespace Espeon.Commands {
	public static class ResponseBuilder {
		private static LocalEmbedBuilder Embed(IMember member, string description, bool isGood) {
			var builder = new LocalEmbedBuilder {
				Author = new LocalEmbedAuthorBuilder() {
					IconUrl = member.GetAvatarUrl(),
					Name = member.DisplayName
				},
				Color = isGood ? Core.Utilities.EspeonColor : new Color(0xff6868),
				Description = description
			};

			return builder;
		}

		public static LocalEmbed Message(EspeonContext context, string message, bool isGood = true) {
			return Embed(context.Member, message, isGood).Build();
		}

		public static LocalEmbed Reminder(IMember member, string message, TimeSpan ago) {
			return Embed(member, message, true).WithFooter($"{ago.Humanize()} ago").Build();
		}
	}
}