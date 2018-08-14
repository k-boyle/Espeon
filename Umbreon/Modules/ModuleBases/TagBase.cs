using Discord.Commands;
using System.Collections.Generic;
using Umbreon.Core.Models.Database;
using Umbreon.Core.Models.Database.Guilds;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
{
    public class TagBase<T> : UmbreonBase<T> where T : class, ICommandContext
    {
        public TagService Tags { get; set; }
        public IEnumerable<Tag> CurrentTags { get; set; }
        public readonly string[] ReservedWords = { "Create", "Modify", "Delete", "List", "Cancel" };

        protected override void BeforeExecute(CommandInfo command)
        {
            CurrentTags = Tags.GetTags(Context);
        }
    }
}
