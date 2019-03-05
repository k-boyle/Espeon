using Discord;
using Espeon.Extensions;
using Qmmands;
using System.Linq;

namespace Espeon.Commands
{
    //TODO make responses look a bit nicer
    public static class ErrorHandling
    {
        private static Color Bad => new Color(0xf31126);

        public static Embed GenerateResponse(this FailedResult result, EspeonContext context)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = context.User.GetAvatarOrDefaultUrl(),
                    Name = context.User.GetDisplayName()
                },
                Color = Bad
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
                        "\n",
                        string.Join('\n', checksFailedResult.FailedChecks.Select(x => x.Error)));

                    builder.WithDescription(message);
                    break;

                //TODO
                case CommandOnCooldownResult commandOnCooldownResult:
                    break;

                //TODO forward to the appropiate authorities
                case ExecutionFailedResult _:
                    builder.WithDescription("Something went horribly wrong... " +
                                            "The problem has been forwarded to the appropiate authorities");
                    break;

                case OverloadsFailedResult overloadsFailedResult:
                    message = string.Concat(
                        result.Reason,
                        "\n",
                        string.Join('\n', overloadsFailedResult.FailedOverloads.Select(x => x.Value.Reason)));

                    builder.WithDescription(message);
                    break;

                case ParameterChecksFailedResult parameterChecksFailedResult:
                    message = string.Concat(
                        result.Reason,
                        "\n",
                        string.Join('\n', parameterChecksFailedResult.FailedChecks.Select(x => x.Error)));

                    builder.WithDescription(message);
                    break;

                case TypeParseFailedResult typeParseFailedResult:
                    builder.WithDescription(typeParseFailedResult.Reason);
                    break;
            }

            return builder.Build();
        }
    }
}
