using Discord;
using System.Linq;

namespace Espeon
{
    public static partial class Utilities
    {
        public static Embed BuildStarMessage(IMessage message)
        {
            string imageUrl = null;

            if (message.Embeds.FirstOrDefault() is IEmbed embed)
            {
                if (embed.Type == EmbedType.Image || embed.Type == EmbedType.Gifv)
                    imageUrl = embed.Url;
            }

            if (message.Attachments.FirstOrDefault() is IAttachment attachment)
            {
                var extensions = new[] { "png", "jpeg", "jpg", "gif", "webp" };

                if (extensions.Any(x => attachment.Url.EndsWith(x)))
                    imageUrl = attachment.Url;
            }

            return BuildStarMessage(message.Author as IGuildUser, message.Content, imageUrl);
        }

        public static Embed BuildStarMessage(IGuildUser user, string content, string imageUrl = null)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = user.GetDisplayName(),
                    IconUrl = user.GetAvatarOrDefaultUrl()
                },
                Description = content,
                Color = Color.Gold
            };

            if (!string.IsNullOrEmpty(imageUrl))
                builder.WithImageUrl(imageUrl);

            return builder.Build();
        }

        public static readonly Emoji Star = new Emoji("⭐");
    }
}
