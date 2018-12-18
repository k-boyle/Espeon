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
            var str = $"Latency: {latency}ms\nPing: ";
            var response = ResponseBuilder.Message(Context, str);

            var sw = new Stopwatch();
            sw.Start();

            var message = await SendMessageAsync(response);

            sw.Stop();

            response = ResponseBuilder.Message(Context, $"{str}{sw.ElapsedMilliseconds}ms");

            await message.ModifyAsync(x => x.Embed = response);
        }
    }
}
