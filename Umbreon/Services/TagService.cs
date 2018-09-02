using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbreon.Attributes;
using Umbreon.Core.Entities.Guild;

namespace Umbreon.Services
{
    [Service]
    public class TagService
    {
        private readonly DatabaseService _database;

        public TagService(DatabaseService database)
        {
            _database = database;
        }

        public void UseTag(ICommandContext context, string tagName)
        {
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.Tags.Find(x => string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase)).Uses++;
            _database.UpdateObject(guild, "guilds");
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
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.Tags.Add(newTag);
            _database.UpdateObject(guild, "guilds");
        }

        public void UpdateTag(ICommandContext context, string tagName, string tagValue)
        {
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.Tags.Find(x => x.TagName == tagName).TagValue = tagValue;
            _database.UpdateObject(guild, "guilds");
        }

        public void DeleteTag(ICommandContext context, string tagName)
        {
            var targetTag = GetTags(context).FirstOrDefault(x =>
                string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase));
            var guild = _database.GetObject<GuildObject>("guilds", context.Guild.Id);
            guild.Tags.Remove(targetTag);
            _database.UpdateObject(guild, "guilds");
        }

        public IEnumerable<Tag> GetTags(ICommandContext context)
            => _database.GetObject<GuildObject>("guilds", context.Guild.Id).Tags;

        public static bool TryParse(IEnumerable<Tag> tags, string tagName, out Tag tag)
        {
            tag = tags.FirstOrDefault(x =>
                string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase));
            return !(tag is null);
        }
    }
}
