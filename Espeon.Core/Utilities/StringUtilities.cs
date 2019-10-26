using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Espeon.Core {
	public static partial class Utilities {
		public static List<string> GetCodes(string content) {
			var strings = new List<string>();
			var sb = new StringBuilder();
			string[] lines = content.Split('\n');
			var languages = new[] {
				"css",
				"asciidoc",
				"autohotkey",
				"bash",
				"coffeescript",
				"cpp",
				"cs",
				"diff",
				"fix",
				"glsl",
				"html",
				"ini",
				"json",
				"md",
				"ml",
				"prolog",
				"py",
				"tex",
				"xl",
				"xml"
			};

			var track = false;

			foreach (string line in lines) {
				if (line.Contains("```")) {
					if (!track) {
						string temp = line.Substring(line.LastIndexOf('`') + 1);
						string[] split = temp.Split(" ");
						if (languages.Any(x => string.Equals(x, split[0], StringComparison.CurrentCultureIgnoreCase))) {
							temp = temp.Replace(split[0], "").TrimStart(' ');
						}

						sb.AppendLine(temp);
						track = true;
					} else {
						if (line[0] != '`') {
							string temp = line.Substring(0,
								line.IndexOf("`", StringComparison.CurrentCultureIgnoreCase));
							sb.AppendLine(temp);
						}

						strings.Add(sb.ToString());
						sb.Clear();
						track = false;
					}
				} else {
					if (track) {
						sb.AppendLine(line);
					}
				}
			}

			return strings;
		}

		public static List<string> SplitByLength(string content, int maxLength) {
			var toReturn = new List<string>();

			string[] split = content.Split(Environment.NewLine);

			var sb = new StringBuilder();

			foreach (string str in split) {
				if (sb.Length + str.Length > maxLength) {
					toReturn.Add(sb.ToString());
					sb.Clear();
				}

				sb.AppendLine(str);
			}

			toReturn.Add(sb.ToString());

			return toReturn;
		}

		private const string Regex =
			@"(\d+)\s?(w(?:eeks|eek?)?|d(?:ays|ay?)?|h(?:ours|rs|r?)|m(?:inutes|ins|in?)?|s(?:econds|econd|ecs|ec?)?)";

		public static readonly Regex TimeSpanRegex = new Regex(Regex, RegexOptions.Compiled);
	}
}