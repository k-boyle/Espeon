using System.Text;

namespace Espeon.Core {
	public static partial class Extensions {
		public static void AppendLine(this StringBuilder sb, object obj) {
			sb.AppendLine(obj.ToString());
		}
	}
}