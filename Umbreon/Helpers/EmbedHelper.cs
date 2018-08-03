using Discord;

namespace Umbreon.Helpers
{
    public static class EmbedHelper
    {
        public static EmbedFieldBuilder EmptyField()
            => new EmbedFieldBuilder
            {
                Name = "\u200b",
                Value = "\u200b"
            };
    }
}
