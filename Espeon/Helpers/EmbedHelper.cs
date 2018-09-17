using Discord;

namespace Espeon.Helpers
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
