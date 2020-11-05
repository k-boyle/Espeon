using Disqord.Bot;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    [Name("Tags")]
    [Description("Store and access frequently used data")]
    public class TagModule : EspeonCommandModule, IAsyncDisposable {
        public EspeonDbContext DbContext { get; set; }
        
        [Name("Guild Tags")]
        [Description("Store and access frequently used data")]
        [Group("tags", "tag", "t")]
        public class GuildTagModule : TagModule {
            [Name("Use Tag")]
            [Description("Finds a tag with the given name, guild specific")]
            [Command]
            public async Task ExecuteTagAsync([Example("espeon")][Remainder] string name) {
                var guildTags = await DbContext.IncludeAndFindAsync<GuildTags, GuildTag, ulong>(
                    Context.Guild.Id.RawValue,
                    tags => tags.Values);
                var tag = guildTags.Values
                    .FirstOrDefault(tag1 => tag1.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                
                if (tag is null) {
                    await ReplyAsync(TAG_DOESNT_EXIST, name);
                    return;
                }

                await ReplyAsync(tag.Value);
                tag.Uses++;
                await DbContext.UpdateAsync(guildTags);
            }
            
            [Name("Create Tag")]
            [Description("Creates a tag with the given name, guild specific")]
            [Command("create", "c")]
            public async Task CreateTagAsync(
                    [Example("espeon")] string name,
                    [Example("is really cool")][Remainder] string value) {
                var module = Context.Command.Module;
                var paths = module.FullAliases.Select(alias => string.Concat(alias, " ", name));
                if (CommandUtilities.EnumerateAllCommands(module).Any(
                    command => command.FullAliases.Any(
                        alias => paths.Any(path => path.Equals(alias, StringComparison.CurrentCultureIgnoreCase))))) {
                    await ReplyAsync(TAG_RESERVED_WORD, name);
                    return;
                }

                var guildTags = await DbContext.IncludeAndFindAsync<GuildTags, GuildTag, ulong>(
                    Context.Guild.Id.RawValue,
                    tags => tags.Values);
                var tag = guildTags.Values
                    .FirstOrDefault(tag1 => tag1.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                
                if (tag != null) {
                    await ReplyAsync(GUILDTAG_ALREADY_EXISTS, name);
                    return;
                }

                guildTags.Values.Add(new GuildTag(Context.Guild.Id, name, value, Context.User.Id));
                await DbContext.UpdateAsync(guildTags);
                await ReplyAsync(TAG_CREATED, name);
            }
            
            //todo moderator override
            [Name("Remove Tag")]
            [Description("Removes a tag with the given name, guild specific")]
            [Command("remove", "rm", "r")]
            public async Task RemoveTagAsync([Example("espeon")] string name) {
                var guildTags = await DbContext.IncludeAndFindAsync<GuildTags, GuildTag, ulong>(
                    Context.Guild.Id.RawValue,
                    tags => tags.Values);
                var tag = guildTags.Values
                    .FirstOrDefault(tag1 => tag1.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));

                if (tag is null) {
                    await ReplyAsync(TAG_DOESNT_EXIST, name);
                    return;
                }
                
                if (tag.OwnerId != Context.Member.Id) {
                    var owner = Context.Guild.Members[tag.OwnerId];
                    await ReplyAsync(MUST_OWN_TAG, name, owner.DisplayName ?? "no one");
                    return;
                }

                await DbContext.RemoveAsync(tag);
                await ReplyAsync(TAG_DELETED, name);
            }

        }
        
        [Name("Global Tags")]
        [Description("Tags that can be access globally, these don't need the tag prefix to execute")]
        [Group("globals", "global", "g")]
        public class GlobalTagModule : TagModule {
            public CommandService Commands { get; set; }
            public ILogger<GlobalTagModule> Logger { get; set; }
            
            [BotOwnerOnly]
            [Name("Create Global Tag")]
            [Description("Creates a global tag")]
            [Command("create", "c")]
            public async Task CreateGlobalTagAsync(
                    [Example("espeon")] string name,
                    [Example("is really cool")][Remainder] string value) {
                var commands = Commands.GetAllCommands();
                if (commands.Any(
                    command => command.FullAliases.Any(
                        alias => alias.Equals(name, StringComparison.CurrentCultureIgnoreCase)))) {
                    await ReplyAsync(TAG_RESERVED_WORD, name);
                    return;
                }

                var tag = new GlobalTag(name, value);
                await DbContext.PersistAsync(tag);

                var tagModule = Commands.TopLevelModules.First(module => module.Type == typeof(TagModule));
                
                Commands.RemoveModule(tagModule);
                await CommandHelper.AddGlobalTagsAsync(DbContext, Commands, Logger);
                
                await ReplyAsync(TAG_CREATED, name);
            }
            
            [BotOwnerOnly]
            [Name("Remove Global Tag")]
            [Description("Removes a global tag")]
            [Command("remove", "rm", "r")]
            public async Task RemoveGlobalTagAsync([Example("espeon")][Remainder] string name) {
                var tag = await DbContext.GlobalTags.FirstOrDefault2Async(
                    globalTag => string.Equals(globalTag.Key, name, StringComparison.CurrentCultureIgnoreCase));
                if (tag is null) {
                    await ReplyAsync(TAG_DOESNT_EXIST, name);
                    return;
                }

                await DbContext.RemoveAsync(tag);

                var tagModule = Commands.TopLevelModules.First(module => module.Type == typeof(TagModule));
                
                Commands.RemoveModule(tagModule);
                await CommandHelper.AddGlobalTagsAsync(DbContext, Commands, Logger);
                
                await ReplyAsync(TAG_DELETED, name);
            }
        }

        public async ValueTask DisposeAsync() {
            await DbContext.DisposeAsync();
        }
    }
}