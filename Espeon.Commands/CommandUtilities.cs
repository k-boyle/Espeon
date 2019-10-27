using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Espeon.Commands {
	public static class Utilities {
		public static bool AvailableName(IEnumerable<Command> commands, string name) {
			return !commands.Any(x => x.FullAliases.Any(y =>
				string.Equals(y, name, StringComparison.InvariantCultureIgnoreCase) || x.Module.FullAliases.Any(z =>
					string.Equals(z, name, StringComparison.InvariantCultureIgnoreCase))));
		}

		public static Embed BuildErrorEmbed(FailedResult result, EspeonContext context) {
			var builder = new EmbedBuilder {
				Author = new EmbedAuthorBuilder {
					IconUrl = context.User.GetAvatarOrDefaultUrl(),
					Name = context.User.GetDisplayName()
				},
				Color = new Color(0xff6868)
			};

			string message;

			switch (result) {
				case ArgumentParseFailedResult argumentParseFailedResult:
					if (argumentParseFailedResult.ParserResult is DefaultArgumentParserResult res) {
						switch (res.Failure) {
							case DefaultArgumentParserFailure.UnclosedQuote:
							case DefaultArgumentParserFailure.UnexpectedQuote:
							case DefaultArgumentParserFailure.NoWhitespaceBetweenArguments:
							case DefaultArgumentParserFailure.TooManyArguments:

								int position = res.FailurePosition ??
								               throw new QuahuLiedException("Result.Position");

								int padding = position + context.PrefixUsed.Length +
								              string.Join(' ', context.Path).Length + 2;

								string leftPad = "^".PadLeft(padding, ' ');
								string rightPad = leftPad.PadRight(context.Message.Content.Length, '^');

								message = string.Concat(result.Reason, "\n```", $"\n{context.Message.Content}\n",
									rightPad, "\n```");

								builder.WithDescription(message);
								break;

							case DefaultArgumentParserFailure.TooFewArguments:

								Command cmd = argumentParseFailedResult.Command;
								IReadOnlyList<Parameter> parameters = cmd.Parameters;

								string response = string.Concat(result.Reason, "\n", cmd.FullAliases.First(), " ",
									string.Join(' ', parameters.Select(x => x.Name)));

								builder.WithDescription(response);
								break;
						}
					}

					break;

				case ChecksFailedResult checksFailedResult:
					message = string.Concat(result.Reason, '\n',
						string.Join('\n', checksFailedResult.FailedChecks.Select(x => x.Result.Reason)));

					builder.WithDescription(message);
					break;

				case CommandOnCooldownResult commandOnCooldownResult:
					message = string.Concat("You are currently on cooldown for this command", '\n', "Retry in: ",
						commandOnCooldownResult.Cooldowns.First().RetryAfter.Humanize(1));

					builder.WithDescription(message);
					break;

				case ExecutionFailedResult _:
					builder.WithDescription("Something went horribly wrong... " +
					                        "The problem has been forwarded to the appropiate authorities");
					break;

				case OverloadsFailedResult overloadsFailedResult:
					message = overloadsFailedResult.FailedOverloads.OrderBy(x => x.Key.Priority).Last().Value.Reason;

					builder.WithDescription(message);
					break;

				case ParameterChecksFailedResult parameterChecksFailedResult:
					message = string.Concat(result.Reason, "\n",
						string.Join('\n', parameterChecksFailedResult.FailedChecks.Select(x => x.Result.Reason)));

					builder.WithDescription(message);
					break;

				case TypeParseFailedResult typeParseFailedResult:
					builder.WithDescription(typeParseFailedResult.Reason);
					break;
			}

			return builder.Build();
		}

		public static readonly IReadOnlyDictionary<Type, string> ExampleUsage = new Dictionary<Type, string> {
			[typeof(IGuildUser)] = "@user",
			[typeof(TimeSpan)] = "1day3hrs14mins30s",
			[typeof(Alias)] = "add/remove",
			[typeof(SocketRole)] = "@role",
			[typeof(Emote[])] = "<:espeon:491227561385525248>",
			[typeof(SocketTextChannel)] = "#channel",
			[typeof(ResponsePack)] = "owo",
			[typeof(Face)] = "heads/tails"
		};
	}
}