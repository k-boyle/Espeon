using System.Collections.Generic;
using Discord.Commands;
using Umbreon.Core.Entities.Guild;
using Umbreon.Services;

namespace Umbreon.Commands.ModuleBases
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
