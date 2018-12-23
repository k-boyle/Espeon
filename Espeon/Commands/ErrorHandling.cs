using Discord;
using Qmmands;
using System.Linq;

namespace Espeon.Commands
{
    //TODO make responses look a bit nicer
    public static class ErrorHandling
    {
        private static Color Bad => new Color(0xf31126);

        public static Embed GenerateResponse(this ArgumentParseFailedResult result, EspeonContext context)
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

            switch (result.ArgumentParserFailure)
            {
                case ArgumentParserFailure.UnclosedQuote:
                case ArgumentParserFailure.UnexpectedQuote:
                case ArgumentParserFailure.NoWhitespaceBetweenArguments:
                case ArgumentParserFailure.TooManyArguments:

                    var position = result.Position ?? throw new QuahuLiedException("Result.Position");

                    var message = string.Concat(
                        result.Reason,
                        "\n```",
                        $"{"^".PadLeft(position, ' ')}",
                        "\n```");

                    builder.WithDescription(message);
                    break;

                case ArgumentParserFailure.TooFewArguments:

                    var cmd = result.Command;
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

            return builder.Build();
        }

        public static Embed GenerateResponse(this ChecksFailedResult result, EspeonContext context)
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

            var message = string.Concat(
                result.Reason,
                "\n",
                string.Join('\n', result.FailedChecks.Select(x => x.Error)));

            builder.WithDescription(message);

            return builder.Build();
        }

        public static Embed GenerateResponse(this ExecutionFailedResult result, EspeonContext context)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = context.User.GetAvatarOrDefaultUrl(),
                    Name = context.User.GetDisplayName()
                },
                Color = Bad,
                Description = "Something went horribly wrong... " +
                              "The problem has been forwarded to the appropiate authorities"
            };

            return builder.Build();
        }

        public static Embed GenerateResponse(this OverloadsFailedResult result, EspeonContext context)
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

            var message = string.Concat(
                result.Reason,
                "\n",
                string.Join('\n', result.FailedOverloads.Select(x => x.Value.Reason)));

            builder.WithDescription(message);

            return builder.Build();
        }

        public static Embed GenerateResponse(this ParameterChecksFailedResult result, EspeonContext context)
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

            var message = string.Concat(
                result.Reason,
                "\n",
                string.Join('\n', result.FailedChecks.Select(x => x.Error)));

            builder.WithDescription(message);

            return builder.Build();
        }

        public static Embed GenerateResponse(this TypeParseFailedResult result, EspeonContext context)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = context.User.GetAvatarOrDefaultUrl(),
                    Name = context.User.GetDisplayName()
                },
                Color = Bad,
                Description = result.Reason
            };

            return builder.Build();
        }
    }
}
