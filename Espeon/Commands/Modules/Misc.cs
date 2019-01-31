using Discord;
using Espeon.Interactive.Callbacks;
using Espeon.Interactive.Criteria;
using Qmmands;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    //TODO rewrite ping
    /*
     * Cat Fact
     * Joke
     * Chuck
     * Gif
     * Clear Responses
     * Admins
     * Mods
     * Help
     */
    [Name("Misc")]
    public class Misc : EspeonBase
    {
        [Command("Ping")]
        [Name("Ping")]
        public async Task PingAsync()
        {
            var latency = Context.Client.Latency;
            var str = await Response.GetResponseAsync(Module, Command, ResponsePack, latency, "");
            var response = ResponseBuilder.Message(Context, str);

            var sw = new Stopwatch();
            sw.Start();

            var message = await SendMessageAsync(response);

            sw.Stop();

            str = await Response.GetResponseAsync(Module, Command, ResponsePack, latency, sw.ElapsedMilliseconds);
            response = ResponseBuilder.Message(Context, str);

            await message.ModifyAsync(x => x.Embed = response);

            var deleteCallback = new DeleteCallback(Context, message, new Emoji("🚮"),
                new ReactionFromSourceUser(Context.User.Id));

            await TryAddCallbackAsync(deleteCallback);
        }
    }
}
