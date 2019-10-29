using Disqord;
using System.Collections.Generic;
using System.Linq;

namespace Espeon.Commands {
	public class PaginatorOptions {
		public Dictionary<IEmoji, Control> Controls { get; set; }
		public Dictionary<int, (string Content, LocalEmbed Embed)> Pages { get; set; }

		public PaginatorOptions() : this(new Dictionary<IEmoji, Control>(), new Dictionary<int, (string, LocalEmbed)>()) { }

		public PaginatorOptions(Dictionary<IEmoji, Control> controls, Dictionary<int, (string, LocalEmbed)> pages) {
			Controls = controls;
			Pages = pages.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
		}

		public static PaginatorOptions Default(Dictionary<int, (string, LocalEmbed)> pages) {
			return new PaginatorOptions {
				Pages = pages,
				Controls = new Dictionary<IEmoji, Control> {
					[new LocalEmoji("⏮")] = Control.First,
					[new LocalEmoji("◀")] = Control.Previous,
					[new LocalEmoji("▶")] = Control.Next,
					[new LocalEmoji("⏭")] = Control.Last,
					[new LocalEmoji("🚮")] = Control.Delete,
					[new LocalEmoji("🔢")] = Control.Skip,
					[new LocalEmoji("ℹ")] = Control.Info
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