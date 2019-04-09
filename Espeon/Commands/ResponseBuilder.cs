using Discord;

namespace Espeon.Commands
{
    public static class ResponseBuilder
    {
        private static Embed Embed(IGuildUser user, string description, bool isGood)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatarOrDefaultUrl(),
                    Name = user.GetDisplayName()
                },
                Color = isGood ? Utilities.EspeonColor : new Color(0xff6868),
                Description = description
            };

            return builder.Build();
        }

        public static Embed Message(EspeonContext context, string message, bool isGood = true)
        {
            return Embed(context.User, message, isGood);
        }

        public static Embed Reminder(IGuildUser user, string message)
        {
            return Embed(user, message, true);
        }
    }
}
