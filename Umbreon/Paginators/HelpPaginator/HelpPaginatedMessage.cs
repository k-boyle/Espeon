using Discord;
using System.Collections.Generic;
using Umbreon.Interactive.Paginator;

namespace Umbreon.Paginators.HelpPaginator
{
    public class HelpPaginatedMessage : BasePaginator
    {
        public List<Page> Pages { get; set; } = new List<Page>();
        public EmbedAuthorBuilder Author { get; set; }
        public string Prefix { get; set; }
        public PaginatedAppearanceOptions Options { get; set; }
    }
}
