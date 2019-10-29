using Disqord;
using System.IO;

namespace Espeon.Core.Entities {
	public class NewMessageProperties {
		public string Content { get; set; }
		public LocalEmbed Embed { get; set; }
		public bool IsTTS { get; set; }
		public Stream Stream { get; set; }
		public string FileName { get; set; }
		public bool IsSpoiler { get; set; }
	}
}