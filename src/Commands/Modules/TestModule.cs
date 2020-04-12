using Qmmands;
using System.Threading.Tasks;
using static Espeon.LocalisationKey;

namespace Espeon {
    public class TestModule : EspeonCommandModule {
        [Command("ping")]
        public async Task PingAsync() {
            await SendLocalisedMessageAsync(PING_COMMAND);
        }
    }
}