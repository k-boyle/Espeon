using Qmmands;
using System.Diagnostics;
using System.Threading.Tasks;
using Base = Espeon.Core.Commands.Modules;

namespace Espeon.Commands.Modules
{
    [Name("Misc")]
    public class Misc : Base.Misc
    {
        [Command("Ping")]
        [Name("Ping")]
        public override async Task PingAsync()
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
        }
    }
}
