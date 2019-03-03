using Discord;
using Espeon.Databases.CommandStore;
using Espeon.Databases.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espeon
{
    public static class Utilities
    {
        public static bool AvailableName(IEnumerable<Command> commands, string name)
            => commands.Any(x => x.FullAliases
                .Any(y => string.Equals(y, name, StringComparison.InvariantCultureIgnoreCase)));

        public static IEnumerable<string> GetCodes(string content)
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

        public static Color EspeonColor = new Color(0xd1a9dd);        
    }
}
