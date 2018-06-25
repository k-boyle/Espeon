using Discord.Addons.Interactive;
using Discord.Commands;
using System.Collections.Generic;
using Umbreon.Core.Models.Database;
using Umbreon.Services;

namespace Umbreon.Modules.ModuleBases
{
    public class TagBase<T> : InteractiveBase<T> where T : class, ICommandContext
    {
        public TagService Tags { get; set; }
        public MessageService Message { get; set; }
        public IEnumerable<Tag> CurrentTags { get; set; }

        protected override void BeforeExecute(CommandInfo command)
        {
            CurrentTags = Tags.GetTags(Context);
        }
    }
}
