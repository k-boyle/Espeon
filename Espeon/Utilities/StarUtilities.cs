using Casino.Discord;
using Discord;

namespace Espeon
{
    public static partial class Utilities
    {
        public static Embed BuildStarMessage(IMessage message)
        {
            var imageUrl = GetImageUrl(message);

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
                Description = content,
                Color = Color.Gold
            }.AddField("\u200b", Format.Url("Original Message", jumpUrl));

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
