﻿using Discord;
using System.Collections.Generic;
using Umbreon.Paginators;
using Colour = Discord.Color;

namespace Umbreon.Interactive.Paginator
{
    public class PaginatedMessage : BasePaginator
    {
        public IEnumerable<object> Pages { get; set; }

        public string Content { get; set; } = "";

        public EmbedAuthorBuilder Author { get; set; } = null;
        public Colour Color { get; set; } = Colour.Default;
        public string Title { get; set; } = "";

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}
