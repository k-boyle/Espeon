using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbreon.Core.Models.Database.Guilds;

namespace Umbreon.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> GetCodes(this string content)
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
                            var temp = line.Substring(0, line.IndexOf("`"));
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

        public static CustomFunction GetFunction(this string content)
        {
            var lines = content.Split("\n");
            var function = new CustomFunction { FunctionCallback = string.Join("", content.GetCodes()) };
            foreach (var line in lines)
            {
                if (line.StartsWith("name: ", StringComparison.CurrentCultureIgnoreCase))
                    function.FunctionName = line.Substring(6);
                else if (line.StartsWith("summary: ", StringComparison.CurrentCultureIgnoreCase))
                    function.Summary = line.Substring(8);
                else if (line.StartsWith("private: ", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!bool.TryParse(line.Substring(8), out var res)) return null;
                    function.IsPrivate = res;
                }
                else if (line.StartsWith("guild: ", StringComparison.CurrentCultureIgnoreCase))
                {
                    function.GuildId = 1;
                }
            }
            return function;
        }
    }
}
