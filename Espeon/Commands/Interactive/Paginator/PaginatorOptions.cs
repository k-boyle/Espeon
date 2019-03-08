using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Espeon.Commands
{
    public class PaginatorOptions
    {
        public Dictionary<IEmote, Control> Controls;
        public Dictionary<int, (string Content, Embed Embed)> Pages;

        public PaginatorOptions() : this(new Dictionary<IEmote, Control>(),
            new Dictionary<int, (string, Embed)>())
        {
        }

        public PaginatorOptions(Dictionary<IEmote, Control> controls,
            Dictionary<int, (string, Embed)> pages)
        {
            Controls = controls;
            Pages = pages.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public enum Control
    {
        First,
        Last,
        Previous,
        Next,
        Delete,
        Skip,
        Info
    }
}
