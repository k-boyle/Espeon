using System.Threading.Tasks;
using Qmmands;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    [Name("Responses")]
    [Description("Commands to configure how the bot communicates with you")]
    public class ResponseModule : EspeonCommandModule {
        public EspeonDbContext DbContext { get; set; }

        [Name("owo")]
        [Command("owo")]
        [Description("Sets the bot to respond using owo responses")]
        public async Task OwoAsync() {
            await LocalisationService.UpdateLocalisationAsync(DbContext, Context.Guild.Id, Context.Member.Id, Language.Owo);
            await ReplyAsync(LOCALISATION_SET, "owo");
        }

        [Name("Default")]
        [Command("default")]
        [Description("Sets the bot to respond using default responses")]
        public async Task DefaultAsync() {
            await LocalisationService.UpdateLocalisationAsync(DbContext, Context.Guild.Id, Context.Member.Id, Language.Default);
            await ReplyAsync(LOCALISATION_SET, "default");
        }
    }
}