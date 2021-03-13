using Disqord.Bot;
using Qmmands;

namespace Espeon
{
    public class TestModule : DiscordGuildModuleBase
    {
        [Command("ping")]
        public DiscordCommandResult Ping()
            => Reply("pong");
    }
}