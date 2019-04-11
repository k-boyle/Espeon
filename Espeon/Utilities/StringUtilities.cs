using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Espeon
{
    public static partial class Utilities
    {
        public static List<string> GetCodes(string content)
        {
            var strings = new List<string>();
            var sb = new StringBuilder();
            var lines = content.Split('\n');
            var languages = new[] {"css", "asciidoc", "autohotkey", "bash", "coffeescript",
                "cpp", "cs", "diff", "fix", "glsl", "html", "ini", "json", "md",
                "ml", "prolog", "py", "tex", "xl", "xml"};

            var track = false;

            foreach (var line in lines)
            {
                if (line.Contains("```"))
                {
                    if (!track)
                    {
                        var temp = line.Substring(line.LastIndexOf('`') + 1);
                        var split = temp.Split(" ");
                        if (languages.Any(x => string.Equals(x, split[0], StringComparison.CurrentCultureIgnoreCase)))
                        {
                            temp = temp.Replace(split[0], "").TrimStart(' ');
                        }
                        sb.AppendLine(temp);
                        track = true;
                    }
                    else
                    {
                        if (line[0] != '`')
                        {
                            var temp = line.Substring(0, line.IndexOf("`", StringComparison.CurrentCultureIgnoreCase));
                            sb.AppendLine(temp);
                        }
                        strings.Add(sb.ToString());
                        sb.Clear();
                        track = false;
                    }
                }
                else
                {
                    if (track)
                        sb.AppendLine(line);
                }
            }

            return strings;
        }

        public static List<string> SplitByLength(string content, int maxLength)
        {
            var toReturn = new List<string>();

            var split = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();

            foreach (var str in split)
            {
                if (sb.Length + str.Length > maxLength)
                {
                    toReturn.Add(sb.ToString());
                    sb.Clear();
                }

                sb.AppendLine(str);
            }

            toReturn.Add(sb.ToString());

            return toReturn;
        }
    }
}
