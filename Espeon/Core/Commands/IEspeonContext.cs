using Discord;
using Discord.WebSocket;
using Qmmands;

namespace Espeon.Core.Commands
{
    public interface IEspeonContext : ICommandContext
    {
        DiscordSocketClient Client { get; }
        IUserMessage Message { get; }
        SocketGuildUser User { get; }
        SocketGuild Guild { get; }
        SocketTextChannel Channel { get; }
    }
}
