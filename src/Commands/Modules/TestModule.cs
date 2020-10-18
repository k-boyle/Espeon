using Qmmands;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    [Name("Test")]
    public class TestModule : EspeonCommandModule {
        [Command("ping")]
        public async Task PingAsync() {
            await ReplyAsync(PING_COMMAND);
        }
        
        [Command("<a:pepohyperwhatif:715291110297043005>")]
        public async Task PepoWhatIfAsync() {
            await ReplyAsync("<a:pepohyperwhatif:715291110297043005>");
        }
    }
}