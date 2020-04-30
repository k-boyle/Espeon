using Qmmands;
using System;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    [Name("Tags")]
    [Group("tags", "tag", "t")]
    public class TagModule : EspeonCommandModule, IAsyncDisposable {
        public EspeonDbContext DbContext { get; set; }
        
        [Name("Use Tag")]
        [Command]
        public async Task ExecuteTagAsync([Example("espeon")][Remainder] string name) {
            var tag = await DbContext.GetTagAsync(Context.Guild, name);
            if (tag is null) {
                await ReplyAsync(GUILDTAG_DOESNT_EXIST, args: name);
                return;
            }

            tag.Uses++;
            await DbContext.UpdateAsync(tag);
            await ReplyAsync(tag.Value);
        }
        
        [Name("Create Tag")]
        [Command("create", "c")]
        public async Task CreateTagAsync(
                [Example("espeon")] string name,
                [Example("is really cool")][Remainder] string value) {
            var tag = await DbContext.GetTagAsync(Context.Guild,
                tag => tag.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            
            if (tag != null) {
                await ReplyAsync(GUILDTAG_ALREADY_EXISTS, args: name);
                return;
            }
            
            tag = new GuildTag(Context.Guild.Id, name, value, Context.User.Id);
            await DbContext.PersistTagAsync(tag);
            await ReplyAsync(GUILDTAG_CREATED, args: name);
        }

        public async ValueTask DisposeAsync() {
            await DbContext.DisposeAsync();
        }
    }
}