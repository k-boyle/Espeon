using System.Collections.Generic;
using Discord;
using Discord.Addons.Interactive.Interfaces;
using Discord.Commands;

namespace Umbreon.Controllers.CommandMenu
{
    public class CommandMenuProperties : IPaginatedMessage
    {
        public Dictionary<string, Emoji> Emojis = new Dictionary<string, Emoji>();
        public Dictionary<ModuleInfo, IEnumerable<CommandInfo>> CommandsDictionary;

        public Emoji MoveUp = new Emoji("⬆");
        public Emoji MoveDown = new Emoji("⬇");
        public Emoji Select = new Emoji("✅");
        public Emoji Back = new Emoji("🔙");
        public Emoji Delete = new Emoji("❌");

        public CommandMenuProperties(Dictionary<ModuleInfo, IEnumerable<CommandInfo>> dict)
        {
            Emojis.Add("up", MoveUp);
            Emojis.Add("down", MoveDown);
            Emojis.Add("select", Select);
            Emojis.Add("back", Back);
            Emojis.Add("delete", Delete);

            CommandsDictionary = dict;
        }
    }
}
