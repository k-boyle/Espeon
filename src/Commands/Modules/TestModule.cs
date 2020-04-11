using Qmmands;
using System.Threading.Tasks;

namespace Espeon {
    public class TestModule : EspeonCommandModule {
        [Command("ping")]
        public async Task PingAsync() {
            await Context.Channel.SendMessageAsync("pong");
        }
    }
}