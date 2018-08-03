using Discord;
using System.Collections.Generic;
using Umbreon.Paginators;

namespace Umbreon.Interactive.Paginator
{
    public class PaginatedMessage : BasePaginator
    {
        public IEnumerable<object> Pages { get; set; }

        public string Content { get; set; } = "";

        public EmbedAuthorBuilder Author { get; set; } = null;
        public Color Color { get; set; } = Color.Default;
        public string Title { get; set; } = "";

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}
