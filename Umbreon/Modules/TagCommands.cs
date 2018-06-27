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
using Umbreon.Core.Extensions;
using Umbreon.Helpers;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

namespace Umbreon.Modules
{
    [Group("Tag")]
    [Name("Tag Commands")]
    [RequireEnabled]
    [ModuleType(Module.Tags)]
    [Summary("Allows all users to create tags for the server. Unlike custom commands these have to be prefix'd with tag")]
    [@Remarks("This module can be disabled", "Module Code: Tags")]
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
            [Remainder] string tagName)
        {
            if (Tags.TryParse(CurrentTags, tagName, out var targetTag))
            {
                Tags.UseTag(Context, targetTag.TagName);
                await Message.SendMessageAsync(Context, targetTag.TagValue);
                return;
            }

            var levenTags = CurrentTags.Where(x => StringHelper.CalcLevenshteinDistance(x.TagName, tagName) < 5);
            var containsTags = CurrentTags.Where(x => x.TagName.Contains(tagName));
            var totalTags = levenTags.Concat(containsTags);
            await Message.SendMessageAsync(Context, "Tag not found did you mean?\n" +
                                                     $"{string.Join("\n", totalTags.Select(x => x.TagName))}");
        }

        [Command("List", RunMode = RunMode.Async)]
        [Alias("")]
        [Name("List Tags")]
        [Priority(1)]
        [Summary("List all the available tags for this server")]
        [Usage("tag list")]
        public async Task ListTags()
        {
            if (!CurrentTags.Any())
            {
                await Message.SendMessageAsync(Context, "There are no tags for this server currently");
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
            await Message.SendMessageAsync(Context, null, paginator: paginator);
        }

        [Command("Info")]
        [Name("Tag Info")]
        [Priority(1)]
        [Summary("Get info on the specified tag")]
        [Usage("tag info ServerInfo")]
        public async Task GetInfo(
            [Name("Tag Name")]
            [Summary("The tag you want to get info on")]
            [Remainder]string tagName)
        {
            if (Tags.TryParse(CurrentTags, tagName, out var targetTag))
            {
                var user = Context.Guild.GetUser(targetTag.TagOwner);
                await Message.SendMessageAsync(Context, string.Empty, new EmbedBuilder
                {
                    Title = $"{targetTag.TagName} info",
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
                            Name = targetTag.TagName,
                            Value = $"**Tag Uses**: {targetTag.Uses}\n" +
                                    $"**Created At**: {targetTag.CreatedAt}\n" +
                                    $"**Created By**: {user.GetDisplayName()}\n" +
                                    $"**Is Claimable**? {user is null}"
                        }
                    }
                }.Build());
                return;
            }

            await Message.SendMessageAsync(Context, "Tag not found");
        }

        [Group("Create")]
        [Name("Create Tags")]
        [Summary("Create a custom tag for the server")]
        public class CreateTag : TagCommands
        {
            [Command(RunMode = RunMode.Async)]
            [Name("Create Tag")]
            [Priority(1)]
            [Summary("Starts the create tag process")]
            [Usage("tag create")]
            public async Task Create()
            {
                await Message.SendMessageAsync(Context, "What do you want the tag to be called? [reply with `cancel` to cancel creation]");
                var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var tagName = reply.Content;
                if (CurrentTags.Any(x => string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await Message.SendMessageAsync(Context, "This tag already exists");
                    return;
                }

                if (ReservedWords.Any(x => string.Equals(x, tagName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await Message.SendMessageAsync(Context, "This is a reserved word, tag cannot be created");
                    return;
                }

                await Message.SendMessageAsync(Context, "What do you want the tag response to be? [reply with `cancel` to cancel creation]");
                reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var tagValue = reply.Content;
                Tags.CreateTag(Context, tagName, tagValue);
                await Message.SendMessageAsync(Context, "Tag has been created");
            }

            [Command(RunMode = RunMode.Async)]
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
                    await Message.SendMessageAsync(Context, "This tag already exists");
                    return;
                }

                if (ReservedWords.Any(x => string.Equals(x, tagName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await Message.SendMessageAsync(Context, "This is a reserved word, tag cannot be created");
                    return;
                }

                await Message.SendMessageAsync(Context, "What do you want the tag response to be? [reply with `cancel` to cancel creation]");
                var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var tagValue = reply.Content;
                Tags.CreateTag(Context, tagName, tagValue);
                await Message.SendMessageAsync(Context, "Tag has been created");
            }

            [Command]
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
                    await Message.SendMessageAsync(Context, "This tag already exists");
                    return;
                }

                if (ReservedWords.Any(x => string.Equals(x, tagName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await Message.SendMessageAsync(Context, "This is a reserved word, tag cannot be created");
                    return;
                }

                Tags.CreateTag(Context, tagName, tagValue);
                await Message.SendMessageAsync(Context, "Tag has been created");
            }
        }

        [Group("Modify")]
        [Name("Modify Tags")]
        [Summary("Modify one of your tags")]
        public class ModifyTag : TagCommands
        {
            [Command(RunMode = RunMode.Async)]
            [Name("Modify Tag")]
            [Priority(1)]
            [Summary("Starts the tag modification process")]
            [Usage("tag modify")]
            public async Task Modify()
            {
                await Message.SendMessageAsync(Context, "Which tag do you want to edit? [reply with `cancel` to cancel modification]");
                var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;

                if (Tags.TryParse(CurrentTags, reply.Content, out var targetTag))
                {
                    if (targetTag.TagOwner != Context.User.Id)
                    {
                        await Message.SendMessageAsync(Context, "Only the tag owner can modify this tag");
                        return;
                    }
                    await Message.SendMessageAsync(Context, "What do you want the new response to be? [reply with `cancel` to cancel modification]");
                    reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                    if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                    var newValue = reply.Content;
                    Tags.UpdateTag(Context, targetTag.TagName, newValue);
                    await Message.SendMessageAsync(Context, "Tag has been modified");
                    return;
                }

                await Message.SendMessageAsync(Context, "Tag was not found");
            }

            [Command(RunMode = RunMode.Async)]
            [Name("Modify Tag")]
            [Priority(1)]
            [Summary("Modify the specified tag name")]
            [Usage("tag modify ServerInfo")]
            public async Task Modify(
                [Name("Tag Name")]
                [Summary("The tag you wanna modify")]
                [Remainder]string tagName)
            {
                if (Tags.TryParse(CurrentTags, tagName, out var targetTag))
                {
                    if (targetTag.TagOwner != Context.User.Id)
                    {
                        await Message.SendMessageAsync(Context, "Only the tag owner can modify this tag");
                        return;
                    }
                    await Message.SendMessageAsync(Context, "What do you want the new response to be? [reply with `cancel` to cancel modification]");
                    var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                    if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                    var newValue = reply.Content;
                    Tags.UpdateTag(Context, targetTag.TagName, newValue);
                    await Message.SendMessageAsync(Context, "Tag has been modified");
                    return;
                }

                await Message.SendMessageAsync(Context, "Tag not found");
            }

            [Command]
            [Name("Modify Tag")]
            [Priority(1)]
            [Summary("Modify the specified tag with the given value")]
            [Usage("tag modify ServerInfo Umbreon is the greatest bot")]
            public async Task Modify(
                [Name("Tag Name")]
                [Summary("The name of the tag you want to modify")]
                string tagName, 
                [Name("Tag Value")]
                [Summary("The new value that you want the tag to have")]
                [Remainder] string tagValue)
            {
                if (Tags.TryParse(CurrentTags, tagName, out var targetTag))
                {
                    if (targetTag.TagOwner != Context.User.Id)
                    {
                        await Message.SendMessageAsync(Context, "Only the tag owner can modify this tag");
                        return;
                    }
                    Tags.UpdateTag(Context, targetTag.TagName, tagValue);
                    await Message.SendMessageAsync(Context, "Tag has been modified");
                    return;
                }

                await Message.SendMessageAsync(Context, "Tag not found");
            }
        }

        [Group("Delete")]
        [Name("Delete Tag")]
        [Summary("Delete one of your tags")]
        public class DeleteTag : TagCommands
        {
            [Command(RunMode = RunMode.Async)]
            [Priority(1)]
            [Name("Delete Tag")]
            [Summary("Start the tag deleted process")]
            [Usage("tag delete")]
            public async Task Delete()
            {
                await Message.SendMessageAsync(Context, "Which tag do you want to delete? [reply with `cancel` to cancel modification]");
                var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;

                if (Tags.TryParse(CurrentTags, reply.Content, out var targetTag))
                {
                    if (targetTag.TagOwner != Context.User.Id)
                    {
                        await Message.SendMessageAsync(Context, "Only the tag owner can delete this tag");
                        return;
                    }
                    Tags.DeleteTag(Context, reply.Content);
                    await Message.SendMessageAsync(Context, "Tag has been deleted");
                    return;
                }

                await Message.SendMessageAsync(Context, "Tag was not found");
            }

            [Command]
            [Priority(1)]
            [Name("Delete Tag")]
            [Summary("Delete the specified tag")]
            [Usage("tag delete ServerInfo")]
            public async Task Delete(
                [Name("Tag Name")]
                [Summary("The name of the tag you want to delete")]
                [Remainder] string tagName)
            {
                if (Tags.TryParse(CurrentTags, tagName, out var targetTag))
                {
                    if (targetTag.TagOwner != Context.User.Id)
                    {
                        await Message.SendMessageAsync(Context, "Only the tag owner can delete this tag");
                        return;
                    }
                    Tags.DeleteTag(Context, targetTag.TagName);
                    await Message.SendMessageAsync(Context, "Tag has been deleted");
                    return;
                }

                await Message.SendMessageAsync(Context, "Tag was not found");
            }
        }
    }
}
