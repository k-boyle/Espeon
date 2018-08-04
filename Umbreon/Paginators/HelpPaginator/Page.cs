using Discord;
using System.Collections.Generic;

namespace Umbreon.Paginators.HelpPaginator
{
    public class Page
    {
        public EmbedFieldBuilder Title { get; set; }
        public List<EmbedFieldBuilder> Fields { get; set; }
    }
}
