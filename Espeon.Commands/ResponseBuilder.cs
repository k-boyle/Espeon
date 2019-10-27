using Casino.Discord;
using Discord;
using Humanizer;
using System;

namespace Espeon.Commands {
	public static class ResponseBuilder {
		private static EmbedBuilder Embed(IGuildUser user, string description, bool isGood) {
			var builder = new EmbedBuilder {
				Author = new EmbedAuthorBuilder {
					IconUrl = user.GetAvatarOrDefaultUrl(),
					Name = user.GetDisplayName()
				},
				Color = isGood ? Core.Utilities.EspeonColor : new Color(0xff6868),
				Description = description
			};

			return builder;
		}

		public static Embed Message(EspeonContext context, string message, bool isGood = true) {
			return Embed(context.User, message, isGood).Build();
		}

		public static Embed Reminder(IGuildUser user, string message, TimeSpan ago) {
			return Embed(user, message, true).WithFooter($"{ago.Humanize()} ago").Build();
		}
	}
}