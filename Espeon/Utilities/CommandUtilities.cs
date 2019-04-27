using Casino.Common.Discord.Net;
using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Espeon
{
    public static partial class Utilities
    {
        public static bool AvailableName(IEnumerable<Command> commands, string name)
            => !commands.Any(x => x.FullAliases
                .Any(y => string.Equals(y, name, StringComparison.InvariantCultureIgnoreCase)));

        public static Embed BuildErrorEmbed(FailedResult result, EspeonContext context)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = context.User.GetAvatarOrDefaultUrl(),
                    Name = context.User.GetDisplayName()
                },
                Color = new Color(0xff6868)
            };

            string message;

            switch (result)
            {
                case ArgumentParseFailedResult argumentParseFailedResult:
                    switch (argumentParseFailedResult.ArgumentParserFailure)
                    {
                        case ArgumentParserFailure.UnclosedQuote:
                        case ArgumentParserFailure.UnexpectedQuote:
                        case ArgumentParserFailure.NoWhitespaceBetweenArguments:
                        case ArgumentParserFailure.TooManyArguments:

                            var position = argumentParseFailedResult.Position ??
                                           throw new QuahuLiedException("Result.Position");

                            var padding = position + context.PrefixUsed.Length 
                                                   + string.Join(' ', context.Path).Length + 2;

                            var leftPad = "^".PadLeft(padding, ' ');
                            var rightPad = leftPad.PadRight(context.Message.Content.Length, '^');

                            message = string.Concat(
                                result.Reason,
                                "\n```",
                                $"\n{context.Message.Content}\n",
                                rightPad,
                                "\n```");

                            builder.WithDescription(message);
                            break;

                        case ArgumentParserFailure.TooFewArguments:

                            var cmd = argumentParseFailedResult.Command;
                            var parameters = cmd.Parameters;

                            var response = string.Concat(
                                result.Reason,
                                "\n",
                                cmd.FullAliases.First(),
                                " ",
                                string.Join(' ', parameters.Select(x => x.Name)));

                            builder.WithDescription(response);
                            break;
                    }
                    break;

                case ChecksFailedResult checksFailedResult:
                    message = string.Concat(
                        result.Reason,
                        '\n',
                        string.Join('\n', checksFailedResult.FailedChecks.Select(x => x.Result.Reason)));

                    builder.WithDescription(message);
                    break;

                case CommandOnCooldownResult commandOnCooldownResult:
                    message = string.Concat(
                        "You are currently on cooldown for this command",
                        '\n',
                        "Retry in: ",
                        commandOnCooldownResult.Cooldowns.First().RetryAfter.Humanize(1)
                        );

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
                    message = string.Concat(
                        result.Reason,
                        "\n",
                        string.Join('\n', parameterChecksFailedResult.FailedChecks.Select(x => x.Result.Reason)));

                    builder.WithDescription(message);
                    break;

                case TypeParseFailedResult typeParseFailedResult:
                    builder.WithDescription(typeParseFailedResult.Reason);
                    break;
            }

            return builder.Build();
        }

        public static IReadOnlyDictionary<Type, string> ExampleUsage = new Dictionary<Type, string>
        {
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
