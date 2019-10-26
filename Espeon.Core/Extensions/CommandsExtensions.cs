using Discord;
using Qmmands;
using System.Collections.Generic;

namespace Espeon.Core {
	public static partial class Extensions {
		//Based on https://github.com/discord-net/Discord.Net/blob/dev/src/Discord.Net.Commands/Extensions/MessageExtensions.cs#L45-L62
		public static bool HasMentionPrefix(this IMessage message, IUser user, out string prefix, out string parsed) {
			string content = message.Content;
			parsed = "";
			prefix = "";
			if (content.Length <= 3 || content[0] != '<' || content[1] != '@') {
				return false;
			}

			int endPos = content.IndexOf('>');
			if (endPos == -1) {
				return false;
			}

			if (content.Length < endPos + 2 || content[endPos + 1] != ' ') {
				return false;
			}

			if (!MentionUtils.TryParseUser(content.Substring(0, endPos + 1), out ulong userId)) {
				return false;
			}

			if (userId != user.Id) {
				return false;
			}

			parsed = content.Substring(endPos + 2);

			prefix = user.Mention;
			return true;
		}

		public static void AddAliases(this CommandBuilder builder, IEnumerable<string> aliases) {
			foreach (string alias in aliases) {
				builder.AddAlias(alias);
			}
		}

		public static void AddAliases(this ModuleBuilder builder, IEnumerable<string> aliases) {
			foreach (string alias in aliases) {
				builder.AddAlias(alias);
			}
		}
	}
}