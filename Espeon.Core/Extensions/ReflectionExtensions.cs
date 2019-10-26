using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Core {
	public static partial class Extensions {
		public static List<string> Inspect(this object obj) {
			Type type = obj.GetType();
			string tStr = type.ToString();
			string vStr = obj.ToString();
			PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name)
				.ToArray();

			if (props.Length == 0) {
				return new List<string>();
			}

			var sb = new StringBuilder();

			sb.AppendLine($"{{{type}: '{(Equals(tStr, vStr) ? "No ToString() overload" : vStr)}'}}");
			sb.AppendLine();

			int maxLength = props.Max(x => x.Name.Length);

			foreach (PropertyInfo prop in props) {
				sb.Append($"#{prop.Name.PadRight(maxLength, ' ')} - ");

				object value = null;

				try {
					value = prop.GetValue(obj);
				} catch (TargetInvocationException) //c# bad
				{ }

				string DateTimeNicefy(DateTimeOffset dateTime) {
					return
						$"{dateTime.Day}/{dateTime.Month}/{dateTime.Year} {dateTime.TimeOfDay.ToString("g").Split('.').First()}";
				}

				string NicefyNamspace(Type inType) {
					string str = inType.ToString();

					return str.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
				}

				type = value?.GetType();
				vStr = value?.ToString();

				switch (value) {
					case IEnumerable collection when !(value is string):

						int count = collection.Cast<object>().Count();

						sb.AppendLine($"[{count} item{(count == 1 ? "" : "s")}]");
						break;


					case Enum @enum:

						sb.AppendLine($"[{@enum.Humanize()}]");

						break;

					case string str:

						sb.AppendLine($"[\"{value}\"]");

						break;

					case Task task:

						Type[] returnT = type.GetGenericArguments();

						if (returnT.Length > 0) {
							sb.AppendLine($"[Task<{string.Join(", ", returnT.Select(NicefyNamspace))}>]");
						} else {
							sb.AppendLine($"[Task]");
						}

						break;

					case DateTime dt:

						sb.AppendLine($"[{DateTimeNicefy(dt)}]");

						break;

					case DateTimeOffset dto:

						sb.AppendLine($"[{DateTimeNicefy(dto)}]");

						break;

					default:

						if (!string.Equals(vStr, type?.ToString())) {
							sb.AppendLine($"[{value}]");
						} else {
							if (type?.IsValueType == false) {
								string niceName = NicefyNamspace(type);

								sb.AppendLine($"[{niceName}]");

							} else {
								sb.AppendLine($"[{value}]");
							}
						}

						break;
				}
			}

			List<string> messages = Utilities.SplitByLength(sb.ToString(), 1980);

			return messages;
		}
	}
}