using Discord;
using Espeon.Core;
using Espeon.Core.Commands;

namespace Espeon
{
    public static class ResponseBuilder
    {
        private const uint Good = 0xd1a9dd;
        private const uint Bad = 0xf31126;

        private static Embed Embed(IGuildUser user, string description, bool isGood)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatarOrDefaultUrl(),
                    Name = user.GetDisplayName()
                },
                Color = new Color(isGood ? Good : Bad),
                Description = description
            };

            return builder.Build();
        }

        public static Embed Message(IEspeonContext context, string message, bool isGood = true)
        {
            return Embed(context.User, message, isGood);
        }

        public static Embed Reminder(IGuildUser user, string message)
        {
            return Embed(user, message, true);
        }
    }
}
