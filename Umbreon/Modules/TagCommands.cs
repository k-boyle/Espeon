using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Net.Helpers;
using MoreLinq;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Core.Extensions;
using Umbreon.Helpers;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

namespace Umbreon.Modules
{
    [Group("Tag")]
    [RequireEnabled]
    [ModuleType(Module.Tags)]
    public class TagCommands : TagBase<GuildCommandContext>
    {
        [Command, Priority(0)]
        public async Task GetTag([Remainder]string tagName)
        {
            var targetTag = CurrentTags.FirstOrDefault(x => string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase));
            if (targetTag != null)
            {
                await Message.SendMessageAsync(Context, targetTag.TagValue);
                return;
            }

            var levenTags = CurrentTags.Where(x => StringHelper.CalcLevenshteinDistance(x.TagName, tagName) < 5);
            var containsTags = CurrentTags.Where(x => x.TagName.Contains(tagName));
            var totalTags = levenTags.Concat(containsTags);
            await Message.SendMessageAsync(Context, "Tag not found did you mean?\n" +
                                                     $"{string.Join("\n", totalTags.Select(x => x.TagName))}");
        }

        [Command("List"), Priority(1)]
        public async Task ListTags()
        {
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
                Options = PaginatedAppearanceOptions.Default,
                Pages = pages
            };
            await Message.SendMessageAsync(Context, null, paginator: paginator);
        }

        [Group("Create")]
        public class CreateTag : TagCommands
        {
            [Command(RunMode = RunMode.Async), Priority(1)]
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
                await Message.SendMessageAsync(Context, "What do you want the tag response to be? [reply with `cancel` to cancel creation]");
                reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var tagValue = reply.Content;
                Tags.CreateTag(Context, tagName, tagValue);
                await Message.SendMessageAsync(Context, "Tag has been created");
            }

            [Command, Priority(1)]
            public async Task Create(string tagName, [Remainder] string tagValue)
            {
                if (CurrentTags.Any(x => string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await Message.SendMessageAsync(Context, "This tag already exists");
                    return;
                }
                Tags.CreateTag(Context, tagName, tagValue);
                await Message.SendMessageAsync(Context, "Tag has been created");
            }
        }

        [Group("Modify")]
        public class ModifyTag : TagCommands
        {
            [Command(RunMode = RunMode.Async), Priority(1)]
            public async Task Modify()
            {
                await Message.SendMessageAsync(Context, "Which tag do you want to edit? [reply with `cancel` to cancel modification]");
                var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var targetTag = CurrentTags.FirstOrDefault(x => string.Equals(x.TagName, reply.Content, StringComparison.CurrentCultureIgnoreCase));
                if (targetTag != null)
                {
                    if(targetTag.TagOwner != Context.User.Id)
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

            [Command(RunMode = RunMode.Async), Priority(1)]
            public async Task Modify([Remainder]string tagName)
            {
                var targetTag = CurrentTags.FirstOrDefault(x =>
                    string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase));
                if (targetTag != null)
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

            [Command, Priority(1)]
            public async Task Modify(string tagName, [Remainder] string tagValue)
            {
                var targetTag = CurrentTags.FirstOrDefault(x =>
                    string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase));
                if (targetTag != null)
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
        public class DeleteTag : TagCommands
        {
            [Command, Priority(1)]
            public async Task Delete()
            {
                await Message.SendMessageAsync(Context, "Which tag do you want to delete? [reply with `cancel` to cancel modification]");
                var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                if (string.Equals(reply.Content, "cancel", StringComparison.CurrentCultureIgnoreCase)) return;
                var targetTag = CurrentTags.FirstOrDefault(x => string.Equals(x.TagName, reply.Content, StringComparison.CurrentCultureIgnoreCase));
                if (targetTag != null)
                {
                    if (targetTag.TagOwner != Context.User.Id)
                    {
                        await Message.SendMessageAsync(Context, "Only the tag owner can modify this tag");
                        return;
                    }
                    Tags.DeleteTag(Context, reply.Content);
                    await Message.SendMessageAsync(Context, "Tag has been deleted");
                    return;
                }

                await Message.SendMessageAsync(Context, "Tag was not found");
            }

            [Command, Priority(1)]
            public async Task Delete([Remainder] string tagName)
            {
                var targetTag = CurrentTags.FirstOrDefault(x => string.Equals(x.TagName, tagName, StringComparison.CurrentCultureIgnoreCase));
                if (targetTag != null)
                {
                    if (targetTag.TagOwner != Context.User.Id)
                    {
                        await Message.SendMessageAsync(Context, "Only the tag owner can modify this tag");
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
