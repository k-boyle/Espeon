using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Espeon.Commands {
	public class PaginatorOptions {
		public Dictionary<IEmote, Control> Controls { get; set; }
		public Dictionary<int, (string Content, Embed Embed)> Pages { get; set; }

		public PaginatorOptions() : this(new Dictionary<IEmote, Control>(), new Dictionary<int, (string, Embed)>()) { }

		public PaginatorOptions(Dictionary<IEmote, Control> controls, Dictionary<int, (string, Embed)> pages) {
			Controls = controls;
			Pages = pages.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
		}

		public static PaginatorOptions Default(Dictionary<int, (string, Embed)> pages) {
			return new PaginatorOptions {
				Pages = pages,
				Controls = new Dictionary<IEmote, Control> {
					[new Emoji("⏮")] = Control.First,
					[new Emoji("◀")] = Control.Previous,
					[new Emoji("▶")] = Control.Next,
					[new Emoji("⏭")] = Control.Last,
					[new Emoji("🚮")] = Control.Delete,
					[new Emoji("🔢")] = Control.Skip,
					[new Emoji("ℹ")] = Control.Info
				}
			};
		}
	}

	public enum Control {
		First,
		Last,
		Previous,
		Next,
		Delete,
		Skip,
		Info
	}
}