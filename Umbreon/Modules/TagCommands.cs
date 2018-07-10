using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net.Helpers;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Models.Database;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

// TODO tag claiming, searching and mod stuff

namespace Umbreon.Modules
{
    [Group("Tag")]
    [Name("Tag Commands")]
    [RequireEnabled]
    [ModuleType(Module.Tags)]
    [Summary("Create tags for your server")]
    [@Remarks("Unlike custom commands anyone can make a tag, and tags must be prefixed with `tag`", "This module can be disabled", "Module Code: Tags")]
    public class TagCommands : TagBase<GuildCommandContext>
    {
        [Command]
        [Name("Get Tag")]
        [Priority(0)]
        [Summary("Gets the requested tag")]
        [Usage("tag ServerInfo")]
        public async Task GetTag(
            [Name("Tag Name")]
            [Summary("The name of the tag you want to fetch")]
            [Remainder] Tag tag)
        {
            Tags.UseTag(Context, tag.TagName);
            await SendMessageAsync(tag.TagValue);
        }

        [Command("List", RunMode = RunMode.Async)]
        [Alias("Tags")]
        [Name("List Tags")]
        [Priority(1)]
        [Summary("List all the available tags for this server")]
        [Usage("tag list")]
        public async Task ListTags()
        {
            if (!CurrentTags.Any())
            {
                await SendMessageAsync("There are no tags for this server currently");
                return;
            }
            var pages = CurrentTags.Select(x => x.TagName).Batch(10).Select(y => string.Join("\n", y));
            var paginator = new PaginatedMessage
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                Color = Color.DarkPurple,
                Title = "Available tags for this server",
                Options = new PaginatedAppearanceOptions(),
                Pages = pages
            };
            await SendMessageAsync(null, paginator: paginator);
        }

        [Command("Info")]
        [Name("Tag Info")]
        [Priority(1)]
        [Summary("Get info on the specified tag")]
        [Usage("tag info ServerInfo")]
        public async Task GetInfo(
            [Name("Tag Name")]
            [Summary("The tag you want to get info on")]
            [Remainder]Tag tag)
        {
            var user = Context.Guild.GetUser(tag.TagOwner);
            await SendMessageAsync(string.Empty, new EmbedBuilder
            {
                Title = $"{tag.TagName} info",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetDefaultAvatarUrl(),
                    Name = user.GetDisplayName()
                },
                Color = Color.Blue,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = tag.TagName,
                        Value = $"**Tag Uses**: {tag.Uses}\n" +
                                $"**Created At**: {tag.CreatedAt}\n" +
                                $"**Created By**: {user.GetDisplayName()}\n" +
                                $"**Is Claimable**? {user is null}"
                    }
                }
            }.Build());
        }

        [Command("Create", RunMode = RunMode.Async)]
        [Name("Create Tag")]
        [Priority(1)]
        [Summary("Starts the create tag process")]
        [Usage("tag create")]
        public async Task Create()
        {
            await SendMessageAsync("What do you want the tag to be called? [reply with `cancel` to cancel creation]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var tagName = reply.Content;
            if (CurrentTags.Any(x => string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This tag already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, tagName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This is a reserved word, tag cannot be created");
                return;
            }

            await SendMessageAsync("What do you want the tag response to be? [reply with `cancel` to cancel creation]");
            reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var tagValue = reply.Content;
            Tags.CreateTag(Context, tagName, tagValue);
            await SendMessageAsync("Tag has been created");
        }

        [Command("Create", RunMode = RunMode.Async)]
        [Name("Create Tag")]
        [Priority(1)]
        [Summary("Creates a tag with the specified name")]
        [Usage("tag create ServerInfo")]
        public async Task Create(
            [Name("Tag Name")]
                [Summary("The name of the tag that you want to create")]string tagName)
        {
            if (CurrentTags.Any(x => string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This tag already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, tagName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This is a reserved word, tag cannot be created");
                return;
            }

            await SendMessageAsync("What do you want the tag response to be? [reply with `cancel` to cancel creation]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var tagValue = reply.Content;
            Tags.CreateTag(Context, tagName, tagValue);
            await SendMessageAsync("Tag has been created");
        }

        [Command("Create")]
        [Name("Create Tag")]
        [Priority(1)]
        [Summary("Creates a tag with the pass parameters")]
        [Usage("tag create ServerInfo This is my server it is for Umbreon")]
        public async Task Create(
            [Name("Tag Name")]
                [Summary("The name of the tag you want to create")]
                string tagName,
            [Name("Tag Value")]
                [Summary("The response you want from the tag")]
                [Remainder] string tagValue)
        {
            if (CurrentTags.Any(x => string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This tag already exists");
                return;
            }

            if (ReservedWords.Any(x => string.Equals(x, tagName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SendMessageAsync("This is a reserved word, tag cannot be created");
                return;
            }

            Tags.CreateTag(Context, tagName, tagValue);
            await SendMessageAsync("Tag has been created");
        }

        [Command("Modify", RunMode = RunMode.Async)]
        [Name("Modify Tag")]
        [Priority(1)]
        [Summary("Starts the tag modification process")]
        [Usage("tag modify")]
        public async Task Modify()
        {
            await SendMessageAsync("Which tag do you want to edit? [reply with `cancel` to cancel modification]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;

            if (Tags.TryParse(CurrentTags, reply.Content, out var targetTag))
            {
                if (targetTag.TagOwner != Context.User.Id)
                {
                    await SendMessageAsync("Only the tag owner can modify this tag");
                    return;
                }
                await SendMessageAsync("What do you want the new response to be? [reply with `cancel` to cancel modification]");
                reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var newValue = reply.Content;
                Tags.UpdateTag(Context, targetTag.TagName, newValue);
                await SendMessageAsync("Tag has been modified");
                return;
            }

            await SendMessageAsync("Tag was not found");
        }

        [Command("Modify", RunMode = RunMode.Async)]
        [Name("Modify Tag")]
        [Priority(1)]
        [Summary("Modify the specified tag name")]
        [Usage("tag modify ServerInfo")]
        public async Task Modify(
            [Name("Tag Name")]
                [Summary("The tag you wanna modify")]
                [Remainder] Tag tag)
        {
            if (tag.TagOwner != Context.User.Id)
            {
                await SendMessageAsync("Only the tag owner can modify this tag");
                return;
            }
            await SendMessageAsync("What do you want the new response to be? [reply with `cancel` to cancel modification]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
            var newValue = reply.Content;
            Tags.UpdateTag(Context, tag.TagName, newValue);
            await SendMessageAsync("Tag has been modified");
        }

        [Command("Modify")]
        [Name("Modify Tag")]
        [Priority(1)]
        [Summary("Modify the specified tag with the given value")]
        [Usage("tag modify ServerInfo Umbreon is the greatest bot")]
        public async Task Modify(
            [Name("Tag Name")]
                [Summary("The name of the tag you want to modify")]
                Tag tag,
            [Name("Tag Value")]
                [Summary("The new value that you want the tag to have")]
                [Remainder] string tagValue)
        {
            if (tag.TagOwner != Context.User.Id)
            {
                await SendMessageAsync("Only the tag owner can modify this tag");
                return;
            }
            Tags.UpdateTag(Context, tag.TagName, tagValue);
            await SendMessageAsync("Tag has been modified");
        }

        [Command("Delete", RunMode = RunMode.Async)]
        [Priority(1)]
        [Name("Delete Tag")]
        [Summary("Start the tag deleted process")]
        [Usage("tag delete")]
        public async Task Delete()
        {
            await SendMessageAsync("Which tag do you want to delete? [reply with `cancel` to cancel modification]");
            var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;

            if (Tags.TryParse(CurrentTags, reply.Content, out var targetTag))
            {
                if (targetTag.TagOwner != Context.User.Id)
                {
                    await SendMessageAsync("Only the tag owner can delete this tag");
                    return;
                }
                Tags.DeleteTag(Context, reply.Content);
                await SendMessageAsync("Tag has been deleted");
                return;
            }

            await SendMessageAsync("Tag was not found");
        }

        [Command("Delete")]
        [Priority(1)]
        [Name("Delete Tag")]
        [Summary("Delete the specified tag")]
        [Usage("tag delete ServerInfo")]
        public async Task Delete(
            [Name("Tag Name")]
                [Summary("The name of the tag you want to delete")]
                [Remainder] Tag tag)
        {
            if (tag.TagOwner != Context.User.Id)
            {
                await SendMessageAsync("Only the tag owner can delete this tag");
                return;
            }
            Tags.DeleteTag(Context, tag.TagName);
            await SendMessageAsync("Tag has been deleted");
        }

    }
}
