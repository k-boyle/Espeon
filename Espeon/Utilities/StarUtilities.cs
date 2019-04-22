using Discord;
using System.Linq;
using Casino.Common.Discord.Net;

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

            return BuildStarMessage(message.Author, message.Content, message.GetJumpUrl(), imageUrl);
        }

        public static Embed BuildStarMessage(IUser user, string content, string jumpUrl, string imageUrl = null)
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = (user as IGuildUser)?.GetDisplayName() ?? user.Username,
                    IconUrl = user.GetAvatarOrDefaultUrl()
                },
                Description = $"{content}\n\n{Format.Url("Original Message", jumpUrl)}",
                Color = Color.Gold
            };

            if (!string.IsNullOrEmpty(imageUrl))
                builder.WithImageUrl(imageUrl);

            return builder.Build();
        }

        public static string BuildJumpUrl(ulong guildId, ulong channelId, ulong messageId)
        {
            const string baseUrl = "https://discordapp.com/channels";
            return $"{baseUrl}/{guildId}/{channelId}/{messageId}";
        }

        public static readonly Emoji Star = new Emoji("⭐");
    }
}
