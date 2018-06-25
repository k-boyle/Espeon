using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbreon.Core.Models.Database;

namespace Umbreon.Services
{
    public class TagService
    {
        private readonly DatabaseService _database;

        public TagService(DatabaseService database)
        {
            _database = database;
        }

        public void CreateTag(ICommandContext context, string tagName, string tagValue)
        {
            var newTag = new Tag
            {
                TagName = tagName,
                TagValue = tagValue,
                TagOwner = context.User.Id,
                CreatedAt = DateTime.UtcNow,
                Uses = 0
            };
            var guild = _database.GetGuild(context);
            guild.Tags.Add(newTag);
            _database.UpdateGuild(guild);
        }

        public void UpdateTag(ICommandContext context, string tagName, string tagValue)
        {
            var targetTag = GetTags(context).FirstOrDefault(x =>
                string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase));
            var guild = _database.GetGuild(context);
            guild.Tags.Find(x => x.TagName == targetTag.TagName).TagValue = tagValue;
            _database.UpdateGuild(guild);
        }

        public IEnumerable<Tag> GetTags(ICommandContext context)
        {
            return _database.GetGuild(context).Tags;
        }
    }
}
