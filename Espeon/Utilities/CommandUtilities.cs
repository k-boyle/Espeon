using Discord;
using Espeon.Commands;
using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Qommon.Collections;

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

                            message = string.Concat(
                                result.Reason,
                                "\n```",
                                $"\n{context.Message.Content.Replace(context.PrefixUsed, "")}\n",
                                $"{"^".PadLeft(position, ' ')}",
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
            [typeof(IGuildUser)] = "@User",
            [typeof(TimeSpan)] = "1day3hrs14mins30s",
            [typeof(Alias)] = "Add/Remove",
            [typeof(SocketRole)] = "@Role",
            [typeof(Emote[])] = "<:pepehands:2394873298>",
            [typeof(SocketTextChannel)] = "#channel",
            [typeof(ResponsePack)] = "owo",
            [typeof(Face)] = "Heads/Tails"
        };
    }
}
