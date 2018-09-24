using Discord;
using System.Collections.Generic;
using Espeon.Helpers;

namespace Espeon.Extensions
{
    public static class EmbedExtensions
    {
        public static void AddEmptyField(this EmbedBuilder embed, bool isInline = false)
            => embed.AddField(EmbedHelper.EmptyField());

        public static EmbedBuilder AddFields(this EmbedBuilder builder, IEnumerable<EmbedFieldBuilder> fields)
        {
            foreach (var field in fields)
                builder.AddField(field);
            return builder;
        }
    }
}
