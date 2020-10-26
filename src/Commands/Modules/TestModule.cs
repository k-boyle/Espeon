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
    }
}